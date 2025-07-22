using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace SmartRoutine.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IStringLocalizer _localizer;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IStringLocalizerFactory localizerFactory)
    {
        _next = next;
        _logger = logger;
        _localizer = localizerFactory.Create("SharedResource", typeof(Program).Assembly.GetName().Name!);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex, _localizer);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, IStringLocalizer localizer)
    {
        context.Response.ContentType = "application/json";
        string message = exception.Message;
        // Eğer mesaj bir resource key ise çevir, değilse doğrudan göster
        var localized = localizer[message];
        if (!localized.ResourceNotFound)
            message = localized;
        var response = new
        {
            message,
            details = message
        };
        switch (exception)
        {
            case ArgumentException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new { message, details = message };
                break;
            case SmartRoutine.Domain.Exceptions.DomainValidationException:
            case SmartRoutine.Domain.Exceptions.DomainException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new { message, details = message };
                break;
            case UnauthorizedAccessException:
            case Application.Exceptions.UnauthorizedException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response = new { message, details = message };
                break;
            case Application.Exceptions.NotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = new { message, details = message };
                break;
            case Application.Exceptions.ValidationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new { message, details = message };
                break;
            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        await context.Response.WriteAsync(jsonResponse);
    }
} 