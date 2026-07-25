namespace DMS.Application.DTOs.Documents;

public sealed record RequestUploadUrlCommand(
    string FileName,
    string ContentType,
    string DocumentType,
    long SizeBytes,
    string? Notes = null);

public sealed record UploadUrlResult(
    Guid DocumentId,
    string FileKey,
    string UploadUrl,
    DateTime ExpiresAtUtc);
