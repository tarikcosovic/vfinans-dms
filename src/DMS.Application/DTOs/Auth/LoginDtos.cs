namespace DMS.Application.DTOs.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResult(
    Guid UserId,
    string Email,
    string Role,
    string FirstName,
    string LastName,
    string CompanyName);
