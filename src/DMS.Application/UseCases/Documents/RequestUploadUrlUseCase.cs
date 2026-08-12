using DMS.Application.DTOs.Documents;
using DMS.Application.Interfaces;
using DMS.Domain.Entities;
using DMS.Domain.Enums;
using DMS.Domain.Exceptions;

namespace DMS.Application.UseCases.Documents;

public sealed class RequestUploadUrlUseCase(
    IDocumentRepository documents,
    IStorageSigner signer,
    IClock clock)
{
    private const long MaxSizeBytes = 3L * 1024 * 1024;
    private const int MaxUploadsPerHour = 10;
    private static readonly TimeSpan PendingTtl = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan DuplicateWindow = TimeSpan.FromMinutes(10);

    public async Task<UploadUrlResult> ExecuteAsync(
        Guid ownerUserId,
        RequestUploadUrlCommand command,
        CancellationToken ct = default)
    {
        if (command.SizeBytes < 1 || command.SizeBytes > MaxSizeBytes)
            throw new DomainException("Veličina datoteke mora biti između 1 bajta i 3 MB.");

        if (string.IsNullOrWhiteSpace(command.FileName))
            throw new DomainException("Naziv datoteke je obavezan.");

        if (string.IsNullOrWhiteSpace(command.Rename))
            throw new DomainException("Naziv dokumenta je obavezan.");

        if (command.Rename.Trim().Length > 255)
            throw new DomainException("Naziv dokumenta može sadržavati najviše 255 znakova.");

        if (string.IsNullOrWhiteSpace(command.ContentType))
            throw new DomainException("Tip sadržaja je obavezan.");

        if (!Enum.TryParse<DocumentType>(command.DocumentType?.Trim(), true, out var documentType))
            throw new DomainException("Vrsta dokumenta je obavezna i mora biti: KIF, KUF, Izvod ili Ostalo.");

        var now = clock.UtcNow;
        await documents.ExpirePendingOlderThanAsync(now.Subtract(PendingTtl), ct);

        var oneHourAgo = now.AddHours(-1);
        var uploadsLastHour = await documents.CountCreatedSinceAsync(ownerUserId, oneHourAgo, ct);
        if (uploadsLastHour >= MaxUploadsPerHour)
            throw new RateLimitExceededException("Dosegnut je limit za otpremanje: maksimalno 10 datoteka po satu.");

        var normalizedFileName = NormalizeForDuplicateCheck(command.FileName);
        var duplicateExists = await documents.ExistsPotentialDuplicateAsync(
            ownerUserId,
            normalizedFileName,
            command.ContentType.Trim().ToLowerInvariant(),
            command.SizeBytes,
            now.Subtract(DuplicateWindow),
            ct);
        if (duplicateExists)
            throw new DomainException("Izgleda da je ovaj dokument već otpremljen ili je otpremanje još uvijek u toku.");

        var safeFileName = SanitizeFileName(command.FileName);
        var fileKey = $"{ownerUserId:N}/{Guid.NewGuid():N}_{safeFileName}";
        var documentId = Guid.NewGuid();

        var document = Document.CreatePending(
            documentId, ownerUserId, fileKey,
            command.FileName, command.Rename, command.ContentType, documentType, command.SizeBytes,
            now, command.Notes);

        await documents.AddAsync(document, ct);
        await documents.SaveChangesAsync(ct);

        var expiresAt = now.AddMinutes(5);
        var uploadUrl = signer.CreateUploadUrl(fileKey, command.ContentType, expiresAt);

        return new UploadUrlResult(documentId, fileKey, uploadUrl, expiresAt);
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName).Trim();
        if (string.IsNullOrWhiteSpace(name)) return "document.bin";
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }

    private static string NormalizeForDuplicateCheck(string fileName) =>
        Path.GetFileName(fileName).Trim().ToLowerInvariant();
}
