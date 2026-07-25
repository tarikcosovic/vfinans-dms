using DMS.Application.DTOs.Documents;
using DMS.Application.Interfaces;
using DMS.Domain.Constants;
using DMS.Domain.Exceptions;

namespace DMS.Application.UseCases.Documents;

public sealed class GetDownloadUrlUseCase(
    IDocumentRepository documents,
    IStorageSigner signer,
    IClock clock)
{
    public async Task<DownloadUrlResult> ExecuteAsync(
        Guid documentId,
        Guid requestingUserId,
        string requestingUserRole,
        CancellationToken ct = default)
    {
        var document = await documents.FindByIdAsync(documentId, ct)
            ?? throw new NotFoundException($"Dokument {documentId} nije pronađen.");

        if (requestingUserRole != RoleNames.Firm && document.OwnerUserId != requestingUserId)
            throw new ForbiddenException();

        if (document.Status.ToString() != "Ready")
            throw new DomainException("Samo spremni dokumenti se mogu preuzeti.");

        if (requestingUserRole == RoleNames.Firm && !document.IsDownloaded)
        {
            document.MarkDownloaded();
            await documents.SaveChangesAsync(ct);
        }

        var expiresAt = clock.UtcNow.AddMinutes(15);
        var url = signer.CreateDownloadUrl(document.FileKey, document.FileName, document.ContentType, expiresAt);

        return new DownloadUrlResult(documentId, url, expiresAt);
    }
}
