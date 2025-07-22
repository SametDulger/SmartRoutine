namespace SmartRoutine.Application.Exceptions;

public abstract class SmartRoutineException : Exception
{
    protected SmartRoutineException(string message) : base(message)
    {
    }

    protected SmartRoutineException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class ValidationException : SmartRoutineException
{
    public ValidationException(string message) : base(message)
    {
    }
}

public class NotFoundException : SmartRoutineException
{
    public NotFoundException(string message) : base(message)
    {
    }
}

public class BusinessException : SmartRoutineException
{
    public BusinessException(string message) : base(message)
    {
    }
}

public class UnauthorizedException : SmartRoutineException
{
    public UnauthorizedException(string message) : base(message) { }
} 