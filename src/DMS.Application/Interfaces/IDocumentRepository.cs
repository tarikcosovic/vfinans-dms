using DMS.Domain.Entities;

namespace DMS.Application.Interfaces;

public interface IDocumentRepository
{
    Task<Document?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Document>> ListByOwnerAsync(Guid ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<Document>> ListAllAsync(CancellationToken ct = default);
    Task<int> CountCreatedSinceAsync(Guid ownerId, DateTime sinceUtc, CancellationToken ct = default);
    Task<int> ExpirePendingOlderThanAsync(DateTime cutoffUtc, CancellationToken ct = default);
    Task<bool> ExistsPotentialDuplicateAsync(
        Guid ownerId,
        string normalizedFileName,
        string contentType,
        long sizeBytes,
        DateTime sinceUtc,
        CancellationToken ct = default);
    Task AddAsync(Document document, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
