namespace DMS.Application.Interfaces;

public interface IClock
{
    DateTime UtcNow { get; }
}
