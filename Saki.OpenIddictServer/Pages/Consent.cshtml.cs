using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Saki.OpenIddictServer.Authorization;
using Saki.OpenIddictServer.Resource;
using System.Text.Json;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Saki.OpenIddictServer.Pages
{
    [Authorize] // Must be logged in to grant consent
    public class ConsentModel : PageModel
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly AuthorizationService _authService; // Inject our helper service

        public ConsentModel(IOpenIddictApplicationManager applicationManager, AuthorizationService authService)
        {
            _applicationManager = applicationManager;
            _authService = authService;
        }

        public string ApplicationName { get; set; }

        [BindProperty] // Bound from the form post
        public string? ReturnUrl { get; set; }

        // Store parameters needed for the redirect back to Authorize endpoint
        [BindProperty]
        public string Parameters { get; set; }

        public async Task<IActionResult> OnGetAsync(string returnUrl)
        {
            ReturnUrl = returnUrl;

            // Extract parameters from ReturnUrl (which is the original /connect/authorize request)
            var requestUri = new Uri(Request.Scheme + "://" + Request.Host + returnUrl);
            var parameters = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(requestUri.Query)
                                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // Store parameters as JSON string for the POST request
            Parameters = JsonSerializer.Serialize(parameters);

            var request = new OpenIddictRequest(parameters);
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
            if (application == null)
            {
                // Handle error: client not found
                return Page();
            }
            ApplicationName = await _applicationManager.GetDisplayNameAsync(application) ?? request.ClientId;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string grantButton)
        {
            if (!User.Identity.IsAuthenticated)
            {
                // Should not happen due to [Authorize], but good practice
                return Challenge(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            // Deserialize parameters back
            var parameters = JsonSerializer.Deserialize<Dictionary<string, StringValues>>(Parameters);
            var request = new OpenIddictRequest(parameters);

            var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
            if (application == null)
            {
                // Handle error
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            if (grantButton == Consts.GrantAccessValue)
            {
                // User granted consent. We don't need to store this permanently here,
                // OpenIddict will create an authorization record if needed.
                // The key is to redirect back to the Authorize endpoint *without* the prompt=consent parameter
                // and *with* the user authenticated.

                // Rebuild the return URL without prompt=consent
                parameters.Remove("prompt");
                var redirectUrl = _authService.BuildRedirectUrl(Request, parameters);

                // Sign in again to potentially update claims if needed, though usually not necessary here.
                // await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, User);

                return Redirect(redirectUrl); // Redirect back to Authorize endpoint
            }
            else // User denied consent
            {
                // Redirect back to the client with an access_denied error.
                var properties = new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.AccessDenied,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user denied access to the application."
                });

                // Important: Use Forbid with the OpenIddict scheme to signal denial to OpenIddict
                return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
        }
    //————————————————
    //版权声明：本文为CSDN博主「为自己_带盐」的原创文章，遵循CC 4.0 BY-SA版权协议，转载请附上原文出处链接及本声明。
    //原文链接：https://blog.csdn.net/juanhuge/article/details/148454418
}
}
