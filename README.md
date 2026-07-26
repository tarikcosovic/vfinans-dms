# V Finans DMS

Document management portal for V Finans built with .NET 9, Razor Pages, MySQL, and Cloudflare R2 direct uploads.

## Features

- Client registration/login with firm approval workflow
- Firm admin page for client approval/deactivation/password reset
- Direct-to-R2 uploads (API does not stream file bytes)
- Document preview/download with read/download tracking
- Filters, pagination (10 per page), and role-based document visibility

## Tech Stack

- .NET 9 (Razor Pages)
- Clean Architecture (`Domain`, `Application`, `Infrastructure`, `Web`)
- Entity Framework Core + MySQL
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

Keep placeholders in `src/DMS.Web/appsettings.json`. Set real secrets through environment variables.

Use these variable names:

- `ConnectionStrings__DefaultConnection`
- `R2__AccessKeyId`
- `R2__SecretAccessKey`
- `R2__BucketName`
- `R2__ServiceUrl`
- `Seeding__FirmUsersInitialPassword`
- `ASPNETCORE_ENVIRONMENT=Production`

For Render, add them in **Web Service -> Environment**.  
For local development, prefer `dotnet user-secrets` (or `appsettings.Development.json`, which is gitignored).

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
