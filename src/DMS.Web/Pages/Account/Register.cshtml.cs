using System.ComponentModel.DataAnnotations;
using DMS.Application.DTOs.Auth;
using DMS.Application.UseCases.Auth;
using DMS.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DMS.Web.Pages.Account;

public class RegisterModel(RegisterUseCase register) : PageModel
{
    [BindProperty]
    public RegisterInputModel Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            await register.ExecuteAsync(
                new RegisterRequest(
                    Input.Email,
                    Input.Password,
                    Input.FirstName,
                    Input.LastName,
                    Input.CompanyName),
                ct);

            TempData["SuccessMessage"] = "Zahtjev za registraciju je poslan. Račun će biti aktivan nakon odobrenja računovodstvenog servisa.";
            return RedirectToPage("/Account/Login");
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}

public sealed class RegisterInputModel
{
    [Required(ErrorMessage = "Ime je obavezno.")]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Prezime je obavezno.")]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Naziv kompanije je obavezan.")]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email je obavezan.")]
    [EmailAddress(ErrorMessage = "Unesite ispravnu email adresu.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lozinka je obavezna.")]
    [MinLength(8, ErrorMessage = "Lozinka mora imati najmanje 8 karaktera.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Potvrda lozinke je obavezna.")]
    [Compare(nameof(Password), ErrorMessage = "Lozinke se ne podudaraju.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
