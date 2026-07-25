using DMS.Application.DTOs.Users;
using DMS.Application.Interfaces;
using DMS.Application.UseCases.Users;
using DMS.Domain.Exceptions;
using DMS.Web.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DMS.Web.Pages.Admin;

[Authorize(Policy = PolicyNames.FirmOnly)]
public class ClientsModel(
    ListClientApprovalsUseCase listClientApprovals,
    ApproveClientUseCase approveClient,
    DeactivateClientUseCase deactivateClient,
    SetClientPasswordUseCase setClientPassword,
    ICurrentUser currentUser) : PageModel
{
    public ClientApprovalDashboardDto Dashboard { get; private set; } =
        new([], []);

    public async Task OnGetAsync(CancellationToken ct)
    {
        Dashboard = await listClientApprovals.ExecuteAsync(ct);
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            await approveClient.ExecuteAsync(userId, currentUser.UserId, ct);
            TempData["SuccessMessage"] = "Klijentski račun je odobren.";
        }
        catch (NotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            await deactivateClient.ExecuteAsync(userId, ct);
            TempData["SuccessMessage"] = "Klijentski račun je deaktiviran.";
        }
        catch (NotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSetPasswordAsync(
        Guid userId,
        string newPassword,
        string confirmPassword,
        CancellationToken ct)
    {
        if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
        {
            TempData["ErrorMessage"] = "Lozinke se ne podudaraju.";
            return RedirectToPage();
        }

        try
        {
            await setClientPassword.ExecuteAsync(userId, newPassword, ct);
            TempData["SuccessMessage"] = "Lozinka klijenta je uspješno promijenjena.";
        }
        catch (NotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }
}
