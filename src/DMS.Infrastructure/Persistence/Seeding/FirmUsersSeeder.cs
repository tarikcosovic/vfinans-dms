using DMS.Application.Interfaces;
using DMS.Domain.Entities;
using DMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DMS.Infrastructure.Persistence.Seeding;

public static class FirmUsersSeeder
{
    public static async Task SeedAsync(
        DmsDbContext db,
        IPasswordHasher hasher,
        string initialPassword,
        CancellationToken ct = default)
    {
        var firmUsers = new[]
        {
            new SeedUser("Harun", "Ramić", "harunramic@vfinans.ba", "V Finans"),
            new SeedUser("Almin", "Valjevac", "alminvaljevac@vfinans.ba", "V Finans"),
        };

        var normalizedEmails = firmUsers
            .Select(u => u.Email.Trim().ToLowerInvariant())
            .ToList();

        var existingUsers = await db.Users
            .Where(u => normalizedEmails.Contains(u.Email))
            .ToListAsync(ct);

        var existingMap = existingUsers.ToDictionary(u => u.Email, StringComparer.OrdinalIgnoreCase);
        var usersToAdd = new List<User>();

        foreach (var user in firmUsers)
        {
            var email = user.Email.Trim().ToLowerInvariant();
            if (existingMap.TryGetValue(email, out var existing))
            {
                existing.UpdateFirmProfile(user.FirstName, user.LastName, user.CompanyName);
                existing.SetPassword(hasher.Hash(initialPassword));
                existing.ActivateSystem();
                continue;
            }

            usersToAdd.Add(User.Create(
                Guid.NewGuid(),
                email,
                hasher.Hash(initialPassword),
                UserRole.Firm,
                user.FirstName,
                user.LastName,
                user.CompanyName,
                isActive: true));
        }

        if (usersToAdd.Count == 0)
        {
            await db.SaveChangesAsync(ct);
            return;
        }

        await db.Users.AddRangeAsync(usersToAdd, ct);
        await db.SaveChangesAsync(ct);
    }

    private sealed record SeedUser(
        string FirstName,
        string LastName,
        string Email,
        string CompanyName);
}
