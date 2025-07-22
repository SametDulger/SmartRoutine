using Microsoft.Extensions.Logging;

namespace SmartRoutine.Infrastructure.Services;

public interface ILoggingService
{
    void LogUserRegistration(string email);
    void LogUserLogin(string email);
    void LogRoutineCreated(Guid userId, string routineTitle);
    void LogRoutineCompleted(Guid userId, Guid routineId, string routineTitle);
    void LogError(string message, Exception exception);
}

public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;

    public LoggingService(ILogger<LoggingService> logger)
    {
        _logger = logger;
    }

    public void LogUserRegistration(string email)
    {
        _logger.LogInformation("New user registered: {Email}", email);
    }

    public void LogUserLogin(string email)
    {
        _logger.LogInformation("User logged in: {Email}", email);
    }

    public void LogRoutineCreated(Guid userId, string routineTitle)
    {
        _logger.LogInformation("Routine created - User: {UserId}, Title: {RoutineTitle}", userId, routineTitle);
    }

    public void LogRoutineCompleted(Guid userId, Guid routineId, string routineTitle)
    {
        _logger.LogInformation("Routine completed - User: {UserId}, RoutineId: {RoutineId}, Title: {RoutineTitle}", 
            userId, routineId, routineTitle);
    }

    public void LogError(string message, Exception exception)
    {
        _logger.LogError(exception, "Application error: {Message}", message);
    }
} 