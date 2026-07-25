using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DMS.Application.DTOs.Auth;
using DMS.Application.UseCases.Auth;
using DMS.Domain.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DMS.Web.Pages.Account;

public class LoginModel(LoginUseCase login) : PageModel
{
    [BindProperty]
    public LoginInputModel Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            var result = await login.ExecuteAsync(
                new LoginRequest(Input.Email, Input.Password), ct);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()),
                new Claim(ClaimTypes.Name, result.Email),
                new Claim(ClaimTypes.Role, result.Role),
                new Claim(ClaimTypes.GivenName, result.FirstName),
                new Claim(ClaimTypes.Surname, result.LastName),
                new Claim("company_name", result.CompanyName),
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return RedirectToPage("/Dms/Index");
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}

public sealed class LoginInputModel
{
    [Required(ErrorMessage = "Email je obavezan.")]
    [EmailAddress(ErrorMessage = "Unesite ispravnu email adresu.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lozinka je obavezna.")]
    public string Password { get; set; } = string.Empty;
}
