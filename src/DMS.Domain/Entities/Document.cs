using DMS.Domain.Enums;
using DMS.Domain.Exceptions;

namespace DMS.Domain.Entities;

public sealed class Document
{
    private Document() { }

    public Guid Id { get; private set; }
    public Guid OwnerUserId { get; private set; }
    public string FileKey { get; private set; } = string.Empty;
    public DocumentStatus Status { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public DocumentType DocumentType { get; private set; }
    public long SizeBytes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public string? Notes { get; private set; }
    public bool IsDownloaded { get; private set; }
    public bool IsRead { get; private set; }

    public static Document CreatePending(
        Guid id, Guid ownerUserId, string fileKey,
        string fileName, string contentType, DocumentType documentType, long sizeBytes,
        DateTime createdAtUtc, string? notes = null) =>
        new()
        {
            Id = id,
            OwnerUserId = ownerUserId,
            FileKey = fileKey,
            Status = DocumentStatus.Pending,
            FileName = fileName,
            ContentType = contentType,
            DocumentType = documentType,
            SizeBytes = sizeBytes,
            CreatedAtUtc = createdAtUtc,
            Notes = notes,
            IsDownloaded = false,
            IsRead = false,
        };

    public void Confirm()
    {
        if (Status != DocumentStatus.Pending)
            throw new DomainException("Only pending documents can be confirmed.");
        Status = DocumentStatus.Ready;
    }

    public void MarkFailed() => Status = DocumentStatus.Failed;

    public void MarkDownloaded() => IsDownloaded = true;

    public void MarkRead() => IsRead = true;
}
