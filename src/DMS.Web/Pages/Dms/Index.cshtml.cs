using DMS.Application.DTOs.Documents;
using DMS.Application.Interfaces;
using DMS.Application.UseCases.Documents;
using DMS.Application.UseCases.Users;
using DMS.Domain.Constants;
using DMS.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DMS.Web.Pages.Dms;

[Authorize]
public class IndexModel(
    ListDocumentsUseCase listDocuments,
    RequestUploadUrlUseCase requestUpload,
    ConfirmUploadUseCase confirmUpload,
    GetDownloadUrlUseCase getDownloadUrl,
    GetPreviewUrlUseCase getPreviewUrl,
    DeleteDocumentUseCase deleteDocument,
    ListClientCompanyNamesUseCase listClientCompanyNames,
    ICurrentUser currentUser) : PageModel
{
    private const int PageSize = 10;

    [BindProperty(SupportsGet = true)]
    public string? CompanyName { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? DocumentType { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Year { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public IReadOnlyList<DocumentDto> Documents { get; private set; } = [];
    public IReadOnlyList<string> CompanyOptions { get; private set; } = [];
    public bool IsFirmUser => currentUser.Role == RoleNames.Firm;

    public async Task OnGetAsync(CancellationToken ct)
    {
        PageNumber = Math.Max(1, PageNumber);
        var allDocuments = await listDocuments.ExecuteAsync(
            currentUser.UserId, currentUser.Role, CompanyName, SearchTerm, DocumentType, Year, ct);
        Documents = BuildDocumentListViewModel(allDocuments, IsFirmUser, PageNumber).Documents;
        if (IsFirmUser)
        {
            CompanyOptions = await listClientCompanyNames.ExecuteAsync(ct);
        }
    }

    public async Task<IActionResult> OnGetDocumentsAsync(
        string? companyName,
        string? searchTerm,
        string? documentType,
        int? year,
        int pageNumber = 1,
        CancellationToken ct = default)
    {
        var allDocuments = await listDocuments.ExecuteAsync(
            currentUser.UserId, currentUser.Role, companyName, searchTerm, documentType, year, ct);
        var viewModel = BuildDocumentListViewModel(allDocuments, IsFirmUser, pageNumber);
        Documents = viewModel.Documents;
        return Partial("_DocumentList", viewModel);
    }

    public async Task<IActionResult> OnPostRequestUploadAsync(
        string fileName, string rename, string contentType, string documentType, long sizeBytes, string? notes,
        CancellationToken ct)
    {
        try
        {
            var result = await requestUpload.ExecuteAsync(
                currentUser.UserId,
                new RequestUploadUrlCommand(fileName, rename, contentType, documentType, sizeBytes, notes),
                ct);

            return new JsonResult(new
            {
                result.DocumentId,
                result.UploadUrl,
                result.ExpiresAtUtc,
            });
        }

        catch (RateLimitExceededException ex)
        {
            Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return new JsonResult(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid documentId, CancellationToken ct)
    {
        try
        {
            await deleteDocument.ExecuteAsync(documentId, currentUser.Role, ct);
            return new JsonResult(new { success = true });
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "Dokument nije pronađen." });
        }
        catch (ForbiddenException)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "Nemate dozvolu za brisanje dokumenata." });
        }
    }

    public async Task<IActionResult> OnPostConfirmAsync(Guid documentId, CancellationToken ct)
    {
        try
        {
            await confirmUpload.ExecuteAsync(documentId, currentUser.UserId, currentUser.Role, ct);
            return new JsonResult(new { success = true });
        }
        catch (DomainException ex)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> OnGetDownloadAsync(Guid documentId, CancellationToken ct)
    {
        try
        {
            var result = await getDownloadUrl.ExecuteAsync(
                documentId, currentUser.UserId, currentUser.Role, ct);
            return Redirect(result.DownloadUrl);
        }
        catch (NotFoundException) { return NotFound(); }
        catch (ForbiddenException) { return Forbid(); }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnGetPreviewAsync(Guid documentId, CancellationToken ct)
    {
        try
        {
            var result = await getPreviewUrl.ExecuteAsync(
                documentId, currentUser.UserId, currentUser.Role, ct);
            return Redirect(result.PreviewUrl);
        }
        catch (NotFoundException) { return NotFound(); }
        catch (ForbiddenException) { return Forbid(); }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage();
        }
    }

    private static DocumentListViewModel BuildDocumentListViewModel(
        IReadOnlyList<DocumentDto> allDocuments,
        bool isFirmUser,
        int page)
    {
        var totalCount = allDocuments.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
        var currentPage = Math.Clamp(page, 1, totalPages);
        var pagedDocuments = allDocuments
            .Skip((currentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        return new DocumentListViewModel(
            pagedDocuments,
            isFirmUser,
            currentPage,
            totalPages,
            totalCount,
            PageSize);
    }
}

public sealed record DocumentListViewModel(
    IReadOnlyList<DocumentDto> Documents,
    bool IsFirmUser,
    int CurrentPage,
    int TotalPages,
    int TotalCount,
    int PageSize);
