using DMS.Domain.Entities;

namespace DMS.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(Guid userId, CancellationToken ct = default);
    Task<User?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<User>> ListPendingClientsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<User>> ListActiveClientsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> ListClientCompanyNamesAsync(CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, string>> GetCompanyNamesByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
