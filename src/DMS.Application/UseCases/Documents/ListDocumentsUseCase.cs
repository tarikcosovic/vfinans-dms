using DMS.Application.DTOs.Documents;
using DMS.Application.Interfaces;
using DMS.Domain.Constants;
using DMS.Domain.Enums;

namespace DMS.Application.UseCases.Documents;

public sealed class ListDocumentsUseCase(
    IDocumentRepository documents,
    IUserRepository users,
    IClock clock)
{
    private static readonly TimeSpan PendingTtl = TimeSpan.FromMinutes(5);

    public async Task<IReadOnlyList<DocumentDto>> ExecuteAsync(
        Guid userId,
        string role,
        string? companyName,
        string? searchTerm,
        int? year,
        CancellationToken ct = default)
    {
        var cutoffUtc = clock.UtcNow.Subtract(PendingTtl);
        await documents.ExpirePendingOlderThanAsync(cutoffUtc, ct);

        var docs = role == RoleNames.Firm
            ? await documents.ListAllAsync(ct)
            : await documents.ListByOwnerAsync(userId, ct);

        var visibleDocs = docs.Where(d => d.Status != DocumentStatus.Pending || d.CreatedAtUtc >= cutoffUtc);

        if (year.HasValue)
        {
            visibleDocs = visibleDocs.Where(d => d.CreatedAtUtc.Year == year.Value);
        }

        var docList = visibleDocs.ToList();
        var ownerIds = docList.Select(d => d.OwnerUserId).Distinct().ToList();
        var companyMap = await users.GetCompanyNamesByUserIdsAsync(ownerIds, ct);

        if (role == RoleNames.Firm && !string.IsNullOrWhiteSpace(companyName))
        {
            var selectedCompany = companyName.Trim();
            docList = docList.Where(d =>
            {
                var ownerCompany = companyMap.TryGetValue(d.OwnerUserId, out var c) ? c : string.Empty;
                return ownerCompany.Equals(selectedCompany, StringComparison.OrdinalIgnoreCase);
            }).ToList();
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            docList = docList.Where(d =>
                d.FileName.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return docList.Select(d => new DocumentDto(
            d.Id,
            d.OwnerUserId,
            companyMap.TryGetValue(d.OwnerUserId, out var c) ? c : string.Empty,
            d.FileName,
            d.ContentType,
            d.DocumentType.ToString(),
            d.SizeBytes,
            d.Status.ToString(),
            d.IsDownloaded,
            d.IsRead,
            d.CreatedAtUtc,
            d.Notes)).ToList();
    }
}
