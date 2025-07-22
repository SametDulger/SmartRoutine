namespace SmartRoutine.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class DomainValidationException : DomainException
{
    public DomainValidationException(string message) : base(message)
    {
    }

    public DomainValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message) : base(message)
    {
    }

    public BusinessRuleViolationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class RoutineNotFoundException : DomainException
{
    public RoutineNotFoundException(Guid routineId) 
        : base($"Rutin bulunamadı: {routineId}")
    {
    }
}

public class UserNotFoundException : DomainException
{
    public UserNotFoundException(Guid userId) 
        : base($"Kullanıcı bulunamadı: {userId}")
    {
    }
} 