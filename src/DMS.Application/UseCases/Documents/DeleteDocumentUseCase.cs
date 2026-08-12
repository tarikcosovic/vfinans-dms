using DMS.Application.Interfaces;
using DMS.Domain.Constants;
using DMS.Domain.Exceptions;

namespace DMS.Application.UseCases.Documents;

public sealed class DeleteDocumentUseCase(
    IDocumentRepository documents,
    IStorageSigner signer)
{
    public async Task ExecuteAsync(
        Guid documentId,
        string requestingUserRole,
        CancellationToken ct = default)
    {
        if (requestingUserRole != RoleNames.Firm)
            throw new ForbiddenException();

        var document = await documents.FindByIdAsync(documentId, ct)
            ?? throw new NotFoundException($"Dokument {documentId} nije pronađen.");

        await signer.DeleteObjectAsync(document.FileKey, ct);
        documents.Remove(document);
        await documents.SaveChangesAsync(ct);
    }
}
