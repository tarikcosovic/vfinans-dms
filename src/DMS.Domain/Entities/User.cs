using DMS.Domain.Enums;

namespace DMS.Domain.Entities;

public sealed class User
{
    private User() { }

    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string CompanyName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime? ApprovedAtUtc { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }

    public static User Create(
        Guid id,
        string email,
        string passwordHash,
        UserRole role,
        string firstName,
        string lastName,
        string companyName,
        bool isActive = false) =>
        new()
        {
            Id = id,
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            CompanyName = companyName.Trim(),
            IsActive = isActive,
        };

    public void SetPassword(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    public void UpdateFirmProfile(string firstName, string lastName, string companyName)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        CompanyName = companyName.Trim();
        Role = UserRole.Firm;
    }

    public void Approve(Guid approvedByUserId, DateTime approvedAtUtc)
    {
        IsActive = true;
        ApprovedByUserId = approvedByUserId;
        ApprovedAtUtc = approvedAtUtc;
    }

    public void ActivateSystem()
    {
        IsActive = true;
        ApprovedByUserId = null;
        ApprovedAtUtc = null;
    }

    public void Deactivate()
    {
        IsActive = false;
        ApprovedByUserId = null;
        ApprovedAtUtc = null;
    }
}
