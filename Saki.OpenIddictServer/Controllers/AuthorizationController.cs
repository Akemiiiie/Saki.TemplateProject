using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Saki.OpenIddictServer.Controllers;

public class AuthorizationController : Controller
{
    private readonly IOpenIddictApplicationManager _applicationManager;

    public AuthorizationController(IOpenIddictApplicationManager applicationManager)
    {
        _applicationManager = applicationManager;
    }

    [HttpPost("~/connect/token")]
    // [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request.IsClientCredentialsGrantType())
        {
            // 注意：客户端凭据由 OpenIddict 自动验证：
            // 如果 client_id 或 client_secret 无效，此操作不会被调用。
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                              throw new InvalidOperationException("找不到应用程序。");

            // 创建一个新的 ClaimsIdentity，包含将用于创建 id_token、token 或 code 的声明。
            var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType,
                OpenIddictConstants.Claims.Name, OpenIddictConstants.Claims.Role);

            // 使用 client_id 作为主题标识符。
            identity.SetClaim(OpenIddictConstants.Claims.Subject,
                await _applicationManager.GetClientIdAsync(application));
            identity.SetClaim(OpenIddictConstants.Claims.Name,
                await _applicationManager.GetDisplayNameAsync(application));

            identity.SetDestinations(static claim => claim.Type switch
            {
                // 当授予 "profile" 作用域时，允许 "name" 声明同时存储在访问令牌和身份令牌中
                // （通过调用 principal.SetScopes(...)）。
                OpenIddictConstants.Claims.Name when claim.Subject.HasScope(OpenIddictConstants.Scopes.Profile)
                    => new List<string>
                    {
                        OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken
                    },

                // 否则，仅将声明存储在访问令牌中。
                _ => new List<string> { OpenIddictConstants.Destinations.AccessToken }
            });
            identity.SetResources("service-worker");

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        if (request.IsPasswordGrantType())
        {
            // 验证用户凭证 (实际项目需连接数据库)
            if (request.Username != "admin" || request.Password != "123456")
                return Forbid();
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: OpenIddictConstants.Claims.Name,
                roleType: OpenIddictConstants.Claims.Role);
            // 添加用户声明
            identity.SetAudiences("");
            identity.AddClaim(OpenIddictConstants.Claims.Subject, "user-001");
            identity.AddClaim(OpenIddictConstants.Claims.Name, "Admin");
            identity.AddClaim(OpenIddictConstants.Claims.Role, "Administrator");
            var principal = new ClaimsPrincipal(identity);
            principal.SetResources("service-worker");
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        throw new NotImplementedException("未实现指定的授权类型。");
    }
}