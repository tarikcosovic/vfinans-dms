using DMS.Application.DTOs.Auth;
using DMS.Application.Interfaces;
using DMS.Domain.Exceptions;

namespace DMS.Application.UseCases.Auth;

public sealed class LoginUseCase(
    IUserRepository users,
    IPasswordHasher hasher)
{
    public async Task<LoginResult> ExecuteAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new DomainException("Email i lozinka su obavezni.");

        var user = await users.FindByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct)
            ?? throw new DomainException("Neispravna email adresa ili lozinka.");

        if (!hasher.Verify(request.Password, user.PasswordHash))
            throw new DomainException("Neispravna email adresa ili lozinka.");

        if (!user.IsActive)
            throw new DomainException("Vaš račun čeka odobrenje računovodstvenog servisa.");

        return new LoginResult(
            user.Id,
            user.Email,
            user.Role.ToString(),
            user.FirstName,
            user.LastName,
            user.CompanyName);
    }
}
