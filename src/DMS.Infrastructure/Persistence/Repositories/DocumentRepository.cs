using DMS.Application.Interfaces;
using DMS.Domain.Entities;
using DMS.Domain.Enums;
using DMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DMS.Infrastructure.Persistence.Repositories;

internal sealed class DocumentRepository(DmsDbContext db) : IDocumentRepository
{
    public Task<Document?> FindByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Documents.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IReadOnlyList<Document>> ListByOwnerAsync(Guid ownerId, CancellationToken ct = default)
    {
        var list = await db.Documents
            .Where(d => d.OwnerUserId == ownerId)
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(ct);
        return list;
    }

    public async Task<IReadOnlyList<Document>> ListAllAsync(CancellationToken ct = default)
    {
        var list = await db.Documents
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(ct);
        return list;
    }

    public Task<int> CountCreatedSinceAsync(Guid ownerId, DateTime sinceUtc, CancellationToken ct = default) =>
        db.Documents.CountAsync(d => d.OwnerUserId == ownerId && d.CreatedAtUtc >= sinceUtc, ct);

    public async Task<int> ExpirePendingOlderThanAsync(DateTime cutoffUtc, CancellationToken ct = default)
    {
        var stalePending = await db.Documents
            .Where(d => d.Status == DocumentStatus.Pending && d.CreatedAtUtc < cutoffUtc)
            .ToListAsync(ct);

        foreach (var doc in stalePending)
        {
            doc.MarkFailed();
        }

        if (stalePending.Count == 0)
        {
            return 0;
        }

        await db.SaveChangesAsync(ct);
        return stalePending.Count;
    }

    public Task<bool> ExistsPotentialDuplicateAsync(
        Guid ownerId,
        string normalizedFileName,
        string contentType,
        long sizeBytes,
        DateTime sinceUtc,
        CancellationToken ct = default)
    {
        var normalizedContentType = contentType.Trim().ToLowerInvariant();

        return db.Documents.AnyAsync(d =>
            d.OwnerUserId == ownerId
            && d.CreatedAtUtc >= sinceUtc
            && d.SizeBytes == sizeBytes
            && (d.Status == DocumentStatus.Pending || d.Status == DocumentStatus.Ready)
            && d.FileName.ToLower() == normalizedFileName
            && d.ContentType.ToLower() == normalizedContentType,
            ct);
    }

    public async Task AddAsync(Document document, CancellationToken ct = default) =>
        await db.Documents.AddAsync(document, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
