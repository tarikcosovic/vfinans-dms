namespace DMS.Application.DTOs.Documents;

public sealed record DocumentDto(
    Guid Id,
    Guid OwnerUserId,
    string OwnerCompanyName,
    string FileName,
    string Rename,
    string ContentType,
    string DocumentType,
    long SizeBytes,
    string Status,
    bool IsDownloaded,
    bool IsRead,
    DateTime CreatedAtUtc,
    string? Notes);

public sealed record DownloadUrlResult(
    Guid DocumentId,
    string DownloadUrl,
    DateTime ExpiresAtUtc);

public sealed record PreviewUrlResult(
    Guid DocumentId,
    string PreviewUrl,
    DateTime ExpiresAtUtc);
