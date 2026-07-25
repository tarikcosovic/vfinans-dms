using DMS.Application.Interfaces;
using DMS.Domain.Exceptions;

namespace DMS.Application.UseCases.Users;

public sealed class ChangeOwnPasswordUseCase(
    IUserRepository users,
    IPasswordHasher hasher)
{
    public async Task ExecuteAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(currentPassword))
            throw new DomainException("Trenutna lozinka je obavezna.");

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            throw new DomainException("Nova lozinka mora imati najmanje 8 karaktera.");

        var user = await users.FindByIdAsync(userId, ct)
            ?? throw new DomainException("Korisnik nije pronađen.");

        if (!hasher.Verify(currentPassword, user.PasswordHash))
            throw new DomainException("Trenutna lozinka nije ispravna.");

        user.SetPassword(hasher.Hash(newPassword));
        await users.SaveChangesAsync(ct);
    }
}
