using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Primitives;
using OpenIddict.Abstractions;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Saki.OpenIddictServer.Authorization
{
    public class AuthorizationService
    {
        // 解析 OAuth 请求参数 (Form or Query)
        public IDictionary<string, StringValues> ParseOAuthParameters(HttpContext httpContext, List<string>? excluding = null)
        {
            excluding ??= new List<string>();
            var parameters = new Dictionary<string, StringValues>();

            if (httpContext.Request.HasFormContentType)
            {
                parameters = httpContext.Request.Form
                    .Where(pair => !excluding.Contains(pair.Key))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            else
            {
                parameters = httpContext.Request.Query
                    .Where(pair => !excluding.Contains(pair.Key))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            return parameters;
        }

        // 构建重定向 URL
        public string BuildRedirectUrl(HttpRequest request, IDictionary<string, StringValues> oAuthParameters)
        {
            var queryString = QueryString.Create(oAuthParameters);
            // Combine PathBase, Path, and the new QueryString
            var url = request.PathBase + request.Path + queryString;
            return url;
        }

        // 检查用户认证状态和 MaxAge
        public bool IsUserAuthenticated(AuthenticateResult authenticateResult, OpenIddictRequest request)
        {
            if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
            {
                return false;
            }

            // Check MaxAge
            if (request.MaxAge.HasValue && authenticateResult.Properties?.IssuedUtc != null)
            {
                var maxAgeSeconds = TimeSpan.FromSeconds(request.MaxAge.Value);
                var authenticationDate = authenticateResult.Properties.IssuedUtc.Value;

                if (DateTimeOffset.UtcNow - authenticationDate > maxAgeSeconds)
                {
                    return false; // Authentication is too old
                }
            }

            return true;
        }

        // 决定声明的目标 (Destination)
        public static IEnumerable<string> GetDestinations(ClaimsIdentity identity, Claim claim)
        {
            // By default, claims are not issued in the access token nor in the identity token.
            // Ask OpenIddict to issue the claim in the identity token only if the "openid" scope was granted
            // and if the user controller corresponding to the claim is listed as an OIDC claim.
            if (claim.Type is Claims.Name or Claims.Email or Claims.Role)
            {
                yield return Destinations.AccessToken;

                if (identity.HasScope(Scopes.OpenId))
                {
                    // Only add to ID token if 'openid' scope is present
                    if (claim.Type is Claims.Name && identity.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    if (claim.Type is Claims.Email && identity.HasScope(Scopes.Email))
                        yield return Destinations.IdentityToken;

                    if (claim.Type is Claims.Role && identity.HasScope(Scopes.Roles))
                        yield return Destinations.IdentityToken;
                }
            }
            // Never include the security stamp in the access token, as it's a secret value.
            else if (claim.Type is "aspnet.identity.securitystamp")
            {
                yield break;
            }
            else
            {
                // Default behavior: add to access token if relevant scope is granted
                if (identity.HasScope(Scopes.Profile) && claim.Type is Claims.PreferredUsername or Claims.GivenName or Claims.FamilyName)
                    yield return Destinations.AccessToken;

                // Add other claims to access token if needed based on scopes
                // Example: if (identity.HasScope("custom_scope") && claim.Type == "my_custom_claim") yield return Destinations.AccessToken;

                // By default, claims are not added to the identity token.
                // To add claims to the identity token, ensure the 'openid' scope is granted
                // and map the claim type to a standard OIDC claim or define a custom scope.
            }
        }
    }
}
