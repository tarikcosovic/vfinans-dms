using DMS.Application.Interfaces;

namespace DMS.Application.UseCases.Users;

public sealed class ListClientCompanyNamesUseCase(IUserRepository users)
{
    public Task<IReadOnlyList<string>> ExecuteAsync(CancellationToken ct = default) =>
        users.ListDocumentOwnerCompanyNamesAsync(ct);
}
