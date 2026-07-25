using DMS.Application.DTOs.Auth;
using DMS.Application.Interfaces;
using DMS.Domain.Constants;
using DMS.Domain.Entities;
using DMS.Domain.Enums;
using DMS.Domain.Exceptions;

namespace DMS.Application.UseCases.Auth;

public sealed class RegisterUseCase(
    IUserRepository users,
    IPasswordHasher hasher)
{
    public async Task<RegisterResult> ExecuteAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new DomainException("Email is required.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new DomainException("Password must be at least 8 characters.");

        if (string.IsNullOrWhiteSpace(request.FirstName))
            throw new DomainException("First name is required.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            throw new DomainException("Last name is required.");

        if (string.IsNullOrWhiteSpace(request.CompanyName))
            throw new DomainException("Company name is required.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await users.EmailExistsAsync(normalizedEmail, ct))
            throw new DomainException("A user with this email already exists.");

        var user = User.Create(
            Guid.NewGuid(),
            normalizedEmail,
            hasher.Hash(request.Password),
            UserRole.Client,
            request.FirstName,
            request.LastName,
            request.CompanyName,
            isActive: false);

        await users.AddAsync(user, ct);
        await users.SaveChangesAsync(ct);

        return new RegisterResult(
            user.Id,
            user.Email,
            user.Role.ToString(),
            user.FirstName,
            user.LastName,
            user.CompanyName);
    }
}
