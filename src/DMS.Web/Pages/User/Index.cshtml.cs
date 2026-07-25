using System.Security.Claims;
using DMS.Application.Interfaces;
using DMS.Application.UseCases.Users;
using DMS.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DMS.Web.Pages.User;

[Authorize]
public class IndexModel(
    ICurrentUser currentUser,
    ChangeOwnPasswordUseCase changeOwnPassword) : PageModel
{
    [BindProperty]
    public ChangePasswordInputModel PasswordInput { get; set; } = new();

    public string Email { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string CompanyName { get; private set; } = string.Empty;

    public void OnGet()
    {
        LoadProfileDetails(currentUser);
    }

    public async Task<IActionResult> OnPostChangePasswordAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            LoadProfileDetails(currentUser);
            return Page();
        }

        if (PasswordInput.NewPassword != PasswordInput.ConfirmNewPassword)
        {
            ModelState.AddModelError(nameof(PasswordInput.ConfirmNewPassword), "Lozinke se ne podudaraju.");
            LoadProfileDetails(currentUser);
            return Page();
        }

        try
        {
            await changeOwnPassword.ExecuteAsync(
                currentUser.UserId,
                PasswordInput.CurrentPassword,
                PasswordInput.NewPassword,
                ct);

            TempData["SuccessMessage"] = "Lozinka je uspješno promijenjena.";
            return RedirectToPage();
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            LoadProfileDetails(currentUser);
            return Page();
        }
    }

    private void LoadProfileDetails(ICurrentUser currentUser)
    {
        Role = currentUser.Role;
        Email = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        FirstName = User.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;
        LastName = User.FindFirstValue(ClaimTypes.Surname) ?? string.Empty;
        CompanyName = User.FindFirstValue("company_name") ?? string.Empty;
    }
}

public sealed class ChangePasswordInputModel
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Trenutna lozinka je obavezna.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Nova lozinka je obavezna.")]
    [System.ComponentModel.DataAnnotations.MinLength(8, ErrorMessage = "Nova lozinka mora imati najmanje 8 karaktera.")]
    public string NewPassword { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Potvrda lozinke je obavezna.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
