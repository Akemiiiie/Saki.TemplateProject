using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Saki.RepositoryTemplate.DBClients;
using BC = BCrypt.Net.BCrypt; // Using BCrypt.Net for password hashing

namespace Saki.OpenIddictServer.Pages
{
    public class AuthenticateModel : PageModel
    {
        private readonly EFDbContext _db;

        public AuthenticateModel(EFDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "test1@example.com"; // Default for testing

        [BindProperty]
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "Password123!"; // Default for testing

        [BindProperty(SupportsGet = true)] // Bind from query string on GET
        public string? ReturnUrl { get; set; }

        public string AuthStatus { get; set; } = "";

        public IActionResult OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            // If user is already authenticated, redirect directly if ReturnUrl is present
            if (User.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(ReturnUrl))
            {
                // Potentially dangerous if ReturnUrl is not validated, but OpenIddict handles this later
                // return Redirect(ReturnUrl);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var user = await _db.Users.AsQueryable().FirstOrDefaultAsync(u => u.Email == Email);
            if (user == null || !BC.Verify(Password, user.PasswordHash))
            {
                AuthStatus = "Invalid username or password.";
                return Page();
            }

            // --- User is authenticated, create claims principal --- 
            var claims = new List<Claim>
            {
                // IMPORTANT: The ClaimTypes.NameIdentifier is the unique ID for the user
                // Using email here for simplicity, but user.Id (Guid) is usually better.
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                // Add other claims like roles if needed
                // new Claim(ClaimTypes.Role, "Admin"), 
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                // Allow refresh
                IsPersistent = true,
                //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10) // Example expiration
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            AuthStatus = "Authentication successful.";

            // Redirect back to the OpenIddict authorize endpoint or a default page
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl)) // Security check
            {
                return Redirect(ReturnUrl);
            }
            else
            {
                // Maybe redirect to a user profile page or home page
                return RedirectToPage("/Index");
            }
        }
    }
}
