using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace SmartRoutine.API.Filters;

public class ApiKeyAuthFilter : IAuthorizationFilter
{
    private readonly IConfiguration _configuration;
    private const string ApiKeyHeaderName = "X-API-Key";
    private readonly ILogger<ApiKeyAuthFilter> _logger;

    public ApiKeyAuthFilter(IConfiguration configuration, ILogger<ApiKeyAuthFilter> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var env = context.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var environmentName = env.EnvironmentName.ToLowerInvariant();
        if (IsDevelopment(env))
        {
            _logger.LogInformation("API Key kontrolü development ortamında atlandı.");
            return;
        }
        if (IsTest(env))
        {
            HandleTestEnvironment(context);
            return;
        }
        HandleProductionOrStaging(context);
    }

    private bool IsDevelopment(IWebHostEnvironment env) => env.IsDevelopment();
    private bool IsTest(IWebHostEnvironment env) => env.EnvironmentName.ToLowerInvariant() == "test";

    private void HandleTestEnvironment(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            _logger.LogInformation("Test ortamında API Key header yok, kontrol atlandı.");
            return;
        }
        var configuredApiKey = Environment.GetEnvironmentVariable("SMARTROUTINE_API_KEY") ?? _configuration["ApiKey"];
        if (string.IsNullOrEmpty(configuredApiKey))
        {
            _logger.LogWarning("Test ortamında API Key yapılandırılmamış.");
            context.Result = new UnauthorizedObjectResult(new { message = "API Key yapılandırılmamış" });
            return;
        }
        if (!configuredApiKey.Equals(extractedApiKey))
        {
            _logger.LogWarning("Test ortamında geçersiz API Key denemesi. IP: {IP}", context.HttpContext.Connection.RemoteIpAddress);
            context.Result = new UnauthorizedObjectResult(new { message = "Geçersiz API Key" });
            return;
        }
        _logger.LogInformation("Test ortamında API Key doğrulandı.");
    }

    private void HandleProductionOrStaging(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            _logger.LogWarning("API Key header eksik.");
            context.Result = new UnauthorizedObjectResult(new { message = "API Key gereklidir" });
            return;
        }
        var configuredApiKeyProd = Environment.GetEnvironmentVariable("SMARTROUTINE_API_KEY") ?? _configuration["ApiKey"];
        if (string.IsNullOrEmpty(configuredApiKeyProd))
        {
            _logger.LogError("API Key yapılandırılmamış. Production/Staging ortamında API Key zorunludur!");
            context.Result = new UnauthorizedObjectResult(new { message = "API Key yapılandırılmamış" });
            return;
        }
        if (!configuredApiKeyProd.Equals(extractedApiKey))
        {
            _logger.LogWarning("Geçersiz API Key denemesi. IP: {IP}", context.HttpContext.Connection.RemoteIpAddress);
            context.Result = new UnauthorizedObjectResult(new { message = "Geçersiz API Key" });
            return;
        }
        _logger.LogInformation("API Key doğrulandı.");
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthAttribute : ServiceFilterAttribute
{
    public ApiKeyAuthAttribute() : base(typeof(ApiKeyAuthFilter))
    {
    }
} 