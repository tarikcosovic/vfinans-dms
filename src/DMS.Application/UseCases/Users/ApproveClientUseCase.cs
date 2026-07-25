using DMS.Application.Interfaces;
using DMS.Domain.Enums;
using DMS.Domain.Exceptions;

namespace DMS.Application.UseCases.Users;

public sealed class ApproveClientUseCase(
    IUserRepository users,
    IClock clock)
{
    public async Task ExecuteAsync(Guid clientUserId, Guid approverUserId, CancellationToken ct = default)
    {
        var user = await users.FindByIdAsync(clientUserId, ct)
            ?? throw new NotFoundException($"Korisnik {clientUserId} nije pronađen.");

        if (user.Role != UserRole.Client)
            throw new DomainException("Odobrenje je dostupno samo za klijentske račune.");

        if (!user.IsActive)
        {
            user.Approve(approverUserId, clock.UtcNow);
            await users.SaveChangesAsync(ct);
        }
    }
}
