using DMS.Application.Interfaces;
using DMS.Domain.Entities;
using DMS.Domain.Enums;
using DMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DMS.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(DmsDbContext db) : IUserRepository
{
    public Task<User?> FindByIdAsync(Guid userId, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);

    public Task<User?> FindByEmailAsync(string email, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        db.Users.AnyAsync(u => u.Email == email, ct);

    public async Task<IReadOnlyList<User>> ListPendingClientsAsync(CancellationToken ct = default)
    {
        var list = await db.Users
            .Where(u => u.Role == UserRole.Client && !u.IsActive)
            .OrderBy(u => u.CompanyName)
            .ThenBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(ct);
        return list;
    }

    public async Task<IReadOnlyList<User>> ListActiveClientsAsync(CancellationToken ct = default)
    {
        var list = await db.Users
            .Where(u => u.Role == UserRole.Client && u.IsActive)
            .OrderBy(u => u.CompanyName)
            .ThenBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(ct);
        return list;
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetCompanyNamesByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct = default)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var rows = await db.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.CompanyName })
            .ToListAsync(ct);

        return rows.ToDictionary(x => x.Id, x => x.CompanyName);
    }

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await db.Users.AddAsync(user, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
