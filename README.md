# V Finans DMS

Document management portal for V Finans built with .NET 9, Razor Pages, PostgreSQL, and Cloudflare R2 direct uploads.

## Features

- Client registration/login with firm approval workflow
- Firm admin page for client approval/deactivation/password reset
- Direct-to-R2 uploads (API does not stream file bytes)
- Document preview/download with read/download tracking
- Filters, pagination (10 per page), and role-based document visibility

## Tech Stack

- .NET 9 (Razor Pages)
- Clean Architecture (`Domain`, `Application`, `Infrastructure`, `Web`)
- Entity Framework Core + PostgreSQL
- Cloudflare R2 (S3-compatible presigned URLs)
- HTMX + Bootstrap

## Project Structure

```text
src/
  DMS.Domain/
  DMS.Application/
  DMS.Infrastructure/
  DMS.Web/
```

## Configuration

Set values in `src/DMS.Web/appsettings.json` (or environment-specific overrides):

- `ConnectionStrings:PostgreSql`
- `R2:AccessKeyId`
- `R2:SecretAccessKey`
- `R2:BucketName`
- `R2:ServiceUrl`
- `Seeding:FirmUsersInitialPassword`

Use placeholders in source control and keep real values in local/secure config.

## Run Locally

```bash
dotnet restore
dotnet build DMS.sln
dotnet run --project src/DMS.Web/DMS.Web.csproj
```

The app applies EF migrations automatically on startup.

## Notes

- Default upload limit is 3 MB per file.
- Upload rate limit is 10 files per user per hour.
- Pending uploads older than 5 minutes are auto-expired.
