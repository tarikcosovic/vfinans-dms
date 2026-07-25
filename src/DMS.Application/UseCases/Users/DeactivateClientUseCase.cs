using DMS.Application.Interfaces;
using DMS.Domain.Enums;
using DMS.Domain.Exceptions;

namespace DMS.Application.UseCases.Users;

public sealed class DeactivateClientUseCase(IUserRepository users)
{
    public async Task ExecuteAsync(Guid clientUserId, CancellationToken ct = default)
    {
        var user = await users.FindByIdAsync(clientUserId, ct)
            ?? throw new NotFoundException($"Korisnik {clientUserId} nije pronađen.");

        if (user.Role != UserRole.Client)
            throw new DomainException("Deaktivacija je dostupna samo za klijentske račune.");

        if (user.IsActive)
        {
            user.Deactivate();
            await users.SaveChangesAsync(ct);
        }
    }
}
