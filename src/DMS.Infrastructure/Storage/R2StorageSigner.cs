using Amazon.S3;
using Amazon.S3.Model;
using DMS.Application.Interfaces;

namespace DMS.Infrastructure.Storage;

internal sealed class R2StorageSigner(IAmazonS3 s3, R2Options options) : IStorageSigner
{
    public string CreateUploadUrl(string key, string contentType, DateTime expiresAtUtc) =>
        s3.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = options.BucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = expiresAtUtc,
            ContentType = contentType,
            Protocol = Protocol.HTTPS,
        });

    public string CreateDownloadUrl(string key, string fileName, string contentType, DateTime expiresAtUtc) =>
        s3.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = options.BucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = expiresAtUtc,
            Protocol = Protocol.HTTPS,
            ResponseHeaderOverrides = new ResponseHeaderOverrides
            {
                ContentType = contentType,
                ContentDisposition = BuildContentDisposition(fileName, asAttachment: true),
            },
        });

    public string CreatePreviewUrl(string key, string fileName, string contentType, DateTime expiresAtUtc) =>
        s3.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = options.BucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = expiresAtUtc,
            Protocol = Protocol.HTTPS,
            ResponseHeaderOverrides = new ResponseHeaderOverrides
            {
                ContentType = contentType,
                ContentDisposition = BuildContentDisposition(fileName, asAttachment: false),
            },
        });

    public async Task<bool> ObjectExistsAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await s3.GetObjectMetadataAsync(options.BucketName, key, ct);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    private static string BuildContentDisposition(string fileName, bool asAttachment)
    {
        var baseName = Path.GetFileName(fileName).Trim();
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "document.bin";
        }

        var safeAscii = baseName.Replace("\"", string.Empty);
        var encoded = Uri.EscapeDataString(baseName);
        var disposition = asAttachment ? "attachment" : "inline";
        return $"{disposition}; filename=\"{safeAscii}\"; filename*=UTF-8''{encoded}";
    }
}
