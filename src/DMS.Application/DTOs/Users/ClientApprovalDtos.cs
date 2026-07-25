namespace DMS.Application.DTOs.Users;

public sealed record ClientApprovalItemDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string CompanyName,
    string Email,
    bool IsActive,
    DateTime? ApprovedAtUtc);

public sealed record ClientApprovalDashboardDto(
    IReadOnlyList<ClientApprovalItemDto> PendingClients,
    IReadOnlyList<ClientApprovalItemDto> ActiveClients);
