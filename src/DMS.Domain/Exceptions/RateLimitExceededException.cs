namespace DMS.Domain.Exceptions;

public sealed class RateLimitExceededException(string message) : DomainException(message);
