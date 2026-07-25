using System.ComponentModel.DataAnnotations;

namespace DMS.Application.DTOs.Auth;

public sealed record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, MaxLength(200)] string CompanyName);

public sealed record RegisterResult(
    Guid UserId,
    string Email,
    string Role,
    string FirstName,
    string LastName,
    string CompanyName);
