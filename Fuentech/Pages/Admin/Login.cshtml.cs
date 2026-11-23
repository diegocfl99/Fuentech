
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Fuentech.Pages.Admin
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public LoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public string Password { get; set; } = "";

        public string ErrorMessage { get; set; } = "";

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            string? adminPassword = _configuration["Admin:Password"];

            if (string.IsNullOrEmpty(adminPassword))
            {
                ErrorMessage = "La contraseña no está configurada.";
                return Page();
            }

            byte[] expected = Encoding.UTF8.GetBytes(adminPassword);
            byte[] received = Encoding.UTF8.GetBytes(Password ?? "");

            bool valid = expected.Length == received.Length &&
                CryptographicOperations.FixedTimeEquals(expected, received);

            if (!valid)
            {
                ErrorMessage = "Contraseña incorrecta.";
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Administrador"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return RedirectToPage("/Admin/Index");
        }
    }
}
