using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SmartRoutine.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestGuid = Guid.NewGuid().ToString();

        var requestNameWithGuid = $"{requestName} [{requestGuid}]";

        _logger.LogInformation("[START] {RequestNameWithGuid}: {Request}",
            requestNameWithGuid, JsonSerializer.Serialize(request));

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation("[END] {RequestNameWithGuid}; ExecutionTime={ExecutionTime}ms",
                requestNameWithGuid, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex, "[ERROR] {RequestNameWithGuid}; ExecutionTime={ExecutionTime}ms; Error={Error}",
                requestNameWithGuid, stopwatch.ElapsedMilliseconds, ex.Message);

            throw;
        }
    }
} 