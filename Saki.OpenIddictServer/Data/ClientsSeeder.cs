using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using Saki.ModelTemplate.Bases;
using Saki.RepositoryTemplate.DBClients;
using static OpenIddict.Abstractions.OpenIddictConstants;
using BC = BCrypt.Net.BCrypt; // Using BCrypt.Net for password hashing

public class ClientsSeeder
{
    private readonly EFDbContext _context;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictScopeManager _scopeManager;

    public ClientsSeeder(EFDbContext context, IOpenIddictApplicationManager appManager, IOpenIddictScopeManager scopeManager)
    {
        _context = context;
        _applicationManager = appManager;
        _scopeManager = scopeManager;
    }

    public async Task SeedAsync()
    {
        await _context.Database.EnsureCreatedAsync(); // Ensure DB exists

        await AddScopesAsync();
        await AddWebApiClientAsync();
        await AddMvcClientAsync();
        await AddOidcDebuggerClientAsync();
        await AddInitUsersAsync();
    }

    private async Task AddScopesAsync()
    {
        // Standard scopes
        if (await _scopeManager.FindByNameAsync(Scopes.OpenId) is null)
            await _scopeManager.CreateAsync(new OpenIddictScopeDescriptor { Name = Scopes.OpenId });
        if (await _scopeManager.FindByNameAsync(Scopes.Profile) is null)
            await _scopeManager.CreateAsync(new OpenIddictScopeDescriptor { Name = Scopes.Profile, Resources = { "resource_server_1" } }); // Link profile scope to resource server
        if (await _scopeManager.FindByNameAsync(Scopes.Email) is null)
            await _scopeManager.CreateAsync(new OpenIddictScopeDescriptor { Name = Scopes.Email });
        if (await _scopeManager.FindByNameAsync(Scopes.Roles) is null)
            await _scopeManager.CreateAsync(new OpenIddictScopeDescriptor { Name = Scopes.Roles });
        if (await _scopeManager.FindByNameAsync(Scopes.OfflineAccess) is null)
            await _scopeManager.CreateAsync(new OpenIddictScopeDescriptor { Name = Scopes.OfflineAccess }); // For Refresh Tokens

        // Custom API scope
        if (await _scopeManager.FindByNameAsync("api1") is null)
        {
            await _scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "api1",
                DisplayName = "API 1 Access",
                Resources = { "resource_server_1" } // Associate scope with the resource server audience
            });
        }
    }

    private async Task AddWebApiClientAsync() // For Swagger UI / Postman / SPA
    {
        var clientId = "web-client";
        if (await _applicationManager.FindByClientIdAsync(clientId) is not null) return;

        await _applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654", // Use a strong secret in production!
            ConsentType = ConsentTypes.Explicit, // Or Implicit/Systematic depending on trust
            DisplayName = "Web API Client (Swagger/SPA)",
            RedirectUris =
            {
                // For Swagger UI
                new Uri("https://localhost:7002/swagger/oauth2-redirect.html"), 
                // Add URIs for your SPA if applicable
                // new Uri("http://localhost:4200/callback") 
            },
            PostLogoutRedirectUris =
            {
                new Uri("https://localhost:7002/resources") // Example redirect after logout
            },
            Permissions =
            {
                // Endpoints needed
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                // Permissions.Endpoints.Logout,
                
                // Grant types allowed
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken, // Allow refresh tokens

                // Response types allowed
                Permissions.ResponseTypes.Code,

                // Scopes allowed
                // Permissions.Scopes.OpenId,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Scopes.Roles,
                // Permissions.Scopes.OfflineAccess, // Needed for refresh tokens
                $"{Permissions.Prefixes.Scope}api1" // Custom scope permission
            },
            Requirements =
            {
                // Require PKCE for Authorization Code flow (recommended)
                Requirements.Features.ProofKeyForCodeExchange
            }
        });
    }

    private async Task AddMvcClientAsync()
    {
        var clientId = "mvc";
        if (await _applicationManager.FindByClientIdAsync(clientId) is not null) return;

        await _applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654", // Use a strong secret!
            ConsentType = ConsentTypes.Explicit,
            DisplayName = "MVC Client Application",
            RedirectUris =
            {
                new Uri("https://localhost:7003/callback/login/local")
            },
            PostLogoutRedirectUris =
            {
                new Uri("https://localhost:7003/callback/logout/local")
            },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                // Permissions.Endpoints.Logout,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                // Permissions.Scopes.OpenId,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Scopes.Roles,
                // Permissions.Scopes.OfflineAccess
                // Note: MVC client usually doesn't need direct API scope ('api1')
                // It gets user info via ID token and makes API calls on behalf of the user later.
            },
            Requirements =
            {
                Requirements.Features.ProofKeyForCodeExchange
            }
        });
    }

    private async Task AddOidcDebuggerClientAsync()
    {
        var clientId = "oidc-debugger";
        if (await _applicationManager.FindByClientIdAsync(clientId) is not null) return;

        await _applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            // OIDC Debugger often uses implicit flow or code flow without secret for public clients
            // For code flow with PKCE (recommended), no secret is needed if client type is public.
            // Let's assume code flow with PKCE for this public client.
            // ClientSecret = "...", // Not needed for public client with PKCE
            ClientType = ClientTypes.Public, // Important for PKCE without secret
            ConsentType = ConsentTypes.Explicit,
            DisplayName = "OIDC Debugger Client",
            RedirectUris = { new Uri("https://oidcdebugger.com/debug") },
            PostLogoutRedirectUris = { new Uri("https://oidcdebugger.com/") },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                // Permissions.Endpoints.Logout,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.ResponseTypes.Code,
                // Permissions.Scopes.OpenId,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Scopes.Roles,
                 $"{Permissions.Prefixes.Scope}api1"
            },
            Requirements =
            {
                Requirements.Features.ProofKeyForCodeExchange
            }
        });
    }

    private async Task AddInitUsersAsync()
    {
        if (await _context.Users.AnyAsync()) return; // Only seed if no users exist
        var users = new List<OpenidUser>()
        {
            new OpenidUser
            {
                UserName = "test1",
                Email = "test1@example.com",
                PasswordHash = BC.HashPassword("Password123!"), // Hash the password!
                Mobile = "110",
                Remark = "Initial test user 1"
            },
            new OpenidUser
            {
                UserName = "test2",
                Email = "test2@example.com",
                PasswordHash = BC.HashPassword("Password123!"),
                Mobile = "119",
                Remark = "Initial test user 2"
            }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
    }
}
//————————————————
//版权声明：本文为CSDN博主「为自己_带盐」的原创文章，遵循CC 4.0 BY-SA版权协议，转载请附上原文出处链接及本声明。
//原文链接：https://blog.csdn.net/juanhuge/article/details/148454418