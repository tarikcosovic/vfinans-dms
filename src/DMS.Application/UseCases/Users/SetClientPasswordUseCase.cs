using DMS.Application.Interfaces;
using DMS.Domain.Enums;
using DMS.Domain.Exceptions;

namespace DMS.Application.UseCases.Users;

public sealed class SetClientPasswordUseCase(
    IUserRepository users,
    IPasswordHasher hasher)
{
    public async Task ExecuteAsync(Guid clientUserId, string newPassword, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            throw new DomainException("Nova lozinka mora imati najmanje 8 karaktera.");

        var user = await users.FindByIdAsync(clientUserId, ct)
            ?? throw new NotFoundException($"Korisnik {clientUserId} nije pronađen.");

        if (user.Role != UserRole.Client)
            throw new DomainException("Lozinku možete promijeniti samo klijentskim računima.");

        user.SetPassword(hasher.Hash(newPassword));
        await users.SaveChangesAsync(ct);
    }
}
