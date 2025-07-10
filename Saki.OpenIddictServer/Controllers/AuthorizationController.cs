using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Saki.OpenIddictServer.Authorization;
using Saki.OpenIddictServer.Resource;
using Saki.RepositoryTemplate.DBClients;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Saki.OpenIddictServer.Controllers;

public class AuthorizationController : Controller
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly AuthorizationService _authService;
    private readonly EFDbContext _dbContext; // Inject DbContext if needed for user lookup

    public AuthorizationController(
       IOpenIddictApplicationManager applicationManager,
       IOpenIddictAuthorizationManager authorizationManager,
       IOpenIddictScopeManager scopeManager,
       AuthorizationService authService,
       EFDbContext dbContext)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _authService = authService;
        _dbContext = dbContext;
    }

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken] // Not needed for standard OAuth/OIDC endpoints
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                       throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Try to retrieve the user principal stored in the authentication cookie.
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // If the user principal can't be extracted, redirect the user to the login page.
        if (!_authService.IsUserAuthenticated(result, request))
        {
            // Build the parameters dictionary for the challenge
            var parameters = _authService.ParseOAuthParameters(HttpContext, new List<string> { Parameters.Prompt });
            var returnUrl = _authService.BuildRedirectUrl(HttpContext.Request, parameters);

            return Challenge(new AuthenticationProperties
            {
                RedirectUri = returnUrl // Redirect back here after login
            },
            CookieAuthenticationDefaults.AuthenticationScheme);
        }

        // Retrieve the application details from the database.
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                          throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

        // Retrieve the permanent authorizations associated with the user and the calling client application.
        var client = await _applicationManager.GetIdAsync(application);
        var userId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

        var authorizations = await _authorizationManager.FindAsync(
            subject: userId,
            client: await _applicationManager.GetIdAsync(application),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        // Check consent requirements
        var consentType = await _applicationManager.GetConsentTypeAsync(application);

        switch (consentType)
        {
            // If the consent is explicit, prompt the user for consent.
            case ConsentTypes.Explicit when !authorizations.Any(): // Only prompt if no valid permanent authorization exists
            case ConsentTypes.Explicit when request.HasParameter(Prompts.Consent):
            // If the consent is systematic, automatically grant consent without prompting the user.
            case ConsentTypes.Systematic:
                // Handled below
                break;

            // If the consent type is unknown or invalid, return an error.
            default:
                return Forbid(new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidClient,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Invalid consent type specified for the client application."
                }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // If prompt=login was specified, force the user to log in again.
        if (request.HasParameter(Prompts.Login))
        {
            // To avoid endless login loops, remember the prompt=login parameter was processed.
            var prompt = string.Join(" ", request.RemoveParameter(Prompts.Login).GetParameters());

            var parameters = _authService.ParseOAuthParameters(HttpContext, new List<string> { Parameters.Prompt });
            parameters[Parameters.Prompt] = prompt;
            var returnUrl = _authService.BuildRedirectUrl(HttpContext.Request, parameters);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, CookieAuthenticationDefaults.AuthenticationScheme);
        }

        // If prompt=consent was specified, or if the client requires explicit consent and no permanent authorization exists,
        // redirect the user to the consent page.
        if (request.HasParameter(Prompts.Consent) || (consentType == ConsentTypes.Explicit && !authorizations.Any()))
        {
            var parameters = _authService.ParseOAuthParameters(HttpContext, new List<string> { Parameters.Prompt });
            var returnUrl = _authService.BuildRedirectUrl(HttpContext.Request, parameters);
            // Pass the original request URL (including query string) to the Consent page
            var consentRedirectUrl = $"/Consent?returnUrl={HttpUtility.UrlEncode(returnUrl)}";
            return Redirect(consentRedirectUrl);
        }

        // --- Consent granted (implicitly or previously) --- 

        // Create the claims-based identity that will be used by OpenIddict.
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        // Add claims based on the authenticated user and requested scopes.
        var user = await _dbContext.Users.FindAsync(Guid.Parse(userId)); // Fetch user details if needed
        if (user == null)
        {
            return Forbid(new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user account is no longer available."
            }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        identity.SetClaim(Claims.Subject, userId) // Subject is the unique user ID
                .SetClaim(Claims.Email, user.Email)
                .SetClaim(Claims.Name, user.UserName);
        // Add roles if applicable: .SetClaims(Claims.Role, new[] { "admin", "user" }.ToImmutableArray());

        // Set the list of scopes granted to the client application.
        identity.SetScopes(request.GetScopes());
        var resources = await _scopeManager.GetResourcesAsync(identity.GetScopes());
        identity.SetResources(resources);
        // Automatically create a permanent authorization to avoid prompting for consent every time
        // if the consent type is explicit or systematic.
        if (consentType is ConsentTypes.Explicit or ConsentTypes.Systematic)
        {
            // Find existing or create new authorization
            var authorization = authorizations.LastOrDefault() ?? await _authorizationManager.CreateAsync(
                identity: identity,
                subject: userId,
                client: await _applicationManager.GetIdAsync(application),
                type: AuthorizationTypes.Permanent,
                scopes: identity.GetScopes());

            identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
        }

        identity.SetDestinations(claim => AuthorizationService.GetDestinations(identity, claim));

        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/connect/token"), Produces("application/json")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                       throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal stored in the authorization code/refresh token.
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var principal = result.Principal;

            var userId = principal.GetClaim(Claims.Subject);
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid(new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // Ensure the user account associated with the claims principal is still valid.
            var user = await _dbContext.Users.FindAsync(Guid.Parse(userId)); // Validate user exists
            if (user == null)
            {
                return Forbid(new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer available."
                }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // Create a new claims principal based on the refresh/authorization code principal.
            var identity = new ClaimsIdentity(principal.Claims,
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            // Override the user claims present in the principal in case they changed since the refresh token was issued.
            identity.SetClaim(Claims.Subject, userId)
                    .SetClaim(Claims.Email, user.Email)
                    .SetClaim(Claims.Name, user.UserName);
            // .SetClaims(Claims.Role, ...);

            identity.SetDestinations(claim => AuthorizationService.GetDestinations(identity, claim));

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        // Handle other grant types like client_credentials if needed
        // else if (request.IsClientCredentialsGrantType()) { ... }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    // Note: The UserInfo endpoint is protected by the OpenIddict validation handler.
    // It requires a valid access token containing the 'openid' scope and the 'sub' claim.
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo"), HttpPost("~/connect/userinfo"), Produces("application/json")]
    public async Task<IActionResult> Userinfo()
    {
        var userId = User.GetClaim(Claims.Subject);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The access token is invalid or doesn't contain a subject claim."
                }));
        }

        var user = await _dbContext.Users.FindAsync(Guid.Parse(userId));
        if (user == null)
        {
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user associated with the access token no longer exists."
                }));
        }

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
            [Claims.Subject] = userId
        };

        if (User.HasScope(Scopes.Profile))
        {
            claims[Claims.Name] = user.UserName;
            // Add other profile claims like 'given_name', 'family_name', etc.
        }

        if (User.HasScope(Scopes.Email))
        {
            claims[Claims.Email] = user.Email;
            claims[Claims.EmailVerified] = true; // Assuming email is verified
        }

        if (User.HasScope(Scopes.Roles))
        {
            // claims[Claims.Role] = ... // Add roles if applicable
        }

        // Add other claims based on requested scopes

        return Ok(claims);
    }

    [HttpGet("~/connect/logout")]
    [HttpPost("~/connect/logout")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Ask ASP.NET Core Identity to delete the local and external cookies created
        // when the user agent is redirected from the external identity provider
        // after a successful authentication flow (e.g. Google or Facebook).
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Returning a SignOutResult will ask OpenIddict to redirect the user agent
        // to the post_logout_redirect_uri specified by the client application or to
        // the RedirectUri specified in the authentication properties if none was set.
        return SignOut(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                       properties: new AuthenticationProperties
                       {
                           // Specify the post-logout redirect URI.
                           // If null, OpenIddict will try to use the client's PostLogoutRedirectUris.
                           // If the client has no registered PostLogoutRedirectUris, OpenIddict will display a default page.
                           RedirectUri = "/"
                       });
    }
}
//————————————————
//版权声明：本文为CSDN博主「为自己_带盐」的原创文章，遵循CC 4.0 BY-SA版权协议，转载请附上原文出处链接及本声明。
//原文链接：https://blog.csdn.net/juanhuge/article/details/148454418