using Amazon.S3;
using DMS.Application.Interfaces;
using DMS.Infrastructure.Persistence;
using DMS.Infrastructure.Persistence.Repositories;
using DMS.Infrastructure.Security;
using DMS.Infrastructure.Storage;
using DMS.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection.");

        services.AddDbContext<DmsDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        // Cloudflare R2 storage
        var r2 = configuration.GetSection("R2").Get<R2Options>()
            ?? throw new InvalidOperationException("Missing R2 configuration section.");

        services.AddSingleton(r2);
        services.AddSingleton<IAmazonS3>(_ =>
        {
            var config = new AmazonS3Config
            {
                ServiceURL = r2.ServiceUrl,
                ForcePathStyle = true,
            };
            var creds = new Amazon.Runtime.BasicAWSCredentials(r2.AccessKeyId, r2.SecretAccessKey);
            return new AmazonS3Client(creds, config);
        });
        services.AddScoped<IStorageSigner, R2StorageSigner>();

        // Utilities
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
