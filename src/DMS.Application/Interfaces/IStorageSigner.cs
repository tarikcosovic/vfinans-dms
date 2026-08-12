namespace DMS.Application.Interfaces;

public interface IStorageSigner
{
    string CreateUploadUrl(string key, string contentType, DateTime expiresAtUtc);
    string CreateDownloadUrl(string key, string fileName, string contentType, DateTime expiresAtUtc);
    string CreatePreviewUrl(string key, string fileName, string contentType, DateTime expiresAtUtc);
    Task<bool> ObjectExistsAsync(string key, CancellationToken ct = default);
    Task DeleteObjectAsync(string key, CancellationToken ct = default);
}
