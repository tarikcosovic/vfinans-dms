using DMS.Application;
using DMS.Application.Interfaces;
using DMS.Infrastructure;
using DMS.Infrastructure.Persistence;
using DMS.Infrastructure.Persistence.Seeding;
using DMS.Web.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    RequireProductionSetting("ConnectionStrings:DefaultConnection");
    RequireProductionSetting("R2:AccessKeyId");
    RequireProductionSetting("R2:SecretAccessKey");
    RequireProductionSetting("R2:BucketName");
    RequireProductionSetting("R2:ServiceUrl");
    RequireProductionSetting("Seeding:FirmUsersInitialPassword");
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CookieCurrentUser>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "dms.auth";
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.FirmOnly, p => p.RequireRole(Roles.Firm));
    options.AddPolicy(PolicyNames.ClientOnly, p => p.RequireRole(Roles.Client));
});

builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
builder.Services.AddRazorPages();

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    app.Urls.Add($"http://0.0.0.0:{port}");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DmsDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await db.Database.MigrateAsync();
    var seedPassword = builder.Configuration["Seeding:FirmUsersInitialPassword"] ?? "PromjeniMe123!";
    await FirmUsersSeeder.SeedAsync(db, hasher, seedPassword);
}

app.MapRazorPages();
app.Run();

void RequireProductionSetting(string key)
{
    var value = builder.Configuration[key];
    if (string.IsNullOrWhiteSpace(value) || value.Contains('<') || value.Contains('>'))
    {
        throw new InvalidOperationException($"Missing production configuration for '{key}'. Set it as an environment variable.");
    }
}
