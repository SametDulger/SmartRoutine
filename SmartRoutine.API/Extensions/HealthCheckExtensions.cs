using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartRoutine.Infrastructure.Data;
using System.Text.Json;

namespace SmartRoutine.API.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>("database", tags: new[] { "ready", "db" })
            .AddCheck("self", () => HealthCheckResult.Healthy("API is running"), tags: new[] { "ready" })
            .AddCheck("memory", () =>
            {
                var allocatedMegabytes = GC.GetTotalMemory(false) / (1024 * 1024);
                return allocatedMegabytes > 100 
                    ? HealthCheckResult.Unhealthy($"High memory usage: {allocatedMegabytes}MB")
                    : HealthCheckResult.Healthy($"Memory usage: {allocatedMegabytes}MB");
            }, tags: new[] { "ready" });

        return services;
    }

    public static void UseHealthChecks(this WebApplication app)
    {
        // General health check endpoint
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteResponse
        });

        // Readiness check endpoint
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteResponse
        });

        // Liveness check endpoint
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteResponse
        });
    }

    private static async Task WriteResponse(HttpContext context, HealthReport result)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = result.Status.ToString(),
            checks = result.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                exception = entry.Value.Exception?.Message,
                duration = entry.Value.Duration.ToString()
            }),
            totalDuration = result.TotalDuration.ToString()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
} 