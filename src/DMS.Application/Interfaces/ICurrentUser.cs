namespace DMS.Application.Interfaces;

public interface ICurrentUser
{
    Guid UserId { get; }
    string Role { get; }
    bool IsAuthenticated { get; }
}
