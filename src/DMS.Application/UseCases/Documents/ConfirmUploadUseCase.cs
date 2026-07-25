using DMS.Application.Interfaces;
using DMS.Domain.Constants;
using DMS.Domain.Enums;
using DMS.Domain.Exceptions;

namespace DMS.Application.UseCases.Documents;

public sealed class ConfirmUploadUseCase(
    IDocumentRepository documents,
    IStorageSigner signer,
    IClock clock)
{
    private static readonly TimeSpan PendingTtl = TimeSpan.FromMinutes(5);

    public async Task ExecuteAsync(
        Guid documentId,
        Guid requestingUserId,
        string requestingUserRole,
        CancellationToken ct = default)
    {
        var document = await documents.FindByIdAsync(documentId, ct)
            ?? throw new NotFoundException($"Dokument {documentId} nije pronađen.");

        if (requestingUserRole != RoleNames.Firm && document.OwnerUserId != requestingUserId)
            throw new ForbiddenException();

        var cutoffUtc = clock.UtcNow.Subtract(PendingTtl);
        if (document.Status == DocumentStatus.Pending && document.CreatedAtUtc < cutoffUtc)
        {
            document.MarkFailed();
            await documents.SaveChangesAsync(ct);
            throw new DomainException("Vrijeme za otpremanje je isteklo nakon 5 minuta. Molimo pokušajte ponovo.");
        }

        if (!await signer.ObjectExistsAsync(document.FileKey, ct))
            throw new DomainException("Otpremljena datoteka nije pronađena u pohrani. Prvo završite direktno otpremanje.");

        document.Confirm();
        await documents.SaveChangesAsync(ct);
    }
}
