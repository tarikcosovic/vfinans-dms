namespace DMS.Domain.Exceptions;

public sealed class NotFoundException(string message) : DomainException(message);
