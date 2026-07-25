using DMS.Application.DTOs.Users;
using DMS.Application.Interfaces;

namespace DMS.Application.UseCases.Users;

public sealed class ListClientApprovalsUseCase(IUserRepository users)
{
    public async Task<ClientApprovalDashboardDto> ExecuteAsync(CancellationToken ct = default)
    {
        var pending = await users.ListPendingClientsAsync(ct);
        var active = await users.ListActiveClientsAsync(ct);

        return new ClientApprovalDashboardDto(
            pending.Select(Map).ToList(),
            active.Select(Map).ToList());
    }

    private static ClientApprovalItemDto Map(Domain.Entities.User user) =>
        new(
            user.Id,
            user.FirstName,
            user.LastName,
            user.CompanyName,
            user.Email,
            user.IsActive,
            user.ApprovedAtUtc);
}
