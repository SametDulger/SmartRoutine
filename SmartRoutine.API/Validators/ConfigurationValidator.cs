using System.ComponentModel.DataAnnotations;

namespace SmartRoutine.API.Validators;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    [Required]
    [MinLength(32, ErrorMessage = "JWT SecretKey must be at least 32 characters long")]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required] 
    public string Audience { get; set; } = string.Empty;
}

public class ApiSettings
{
    public const string SectionName = "ApiSettings";
    
    public string? ApiKey { get; set; }
    
    [Range(1, 100)]
    public int RateLimitRequests { get; set; } = 100;
    
    [Range(1, 3600)]
    public int RateLimitWindow { get; set; } = 60; // seconds
}

public static class ConfigurationValidationExtensions
{
    public static IServiceCollection AddConfigurationValidation(this IServiceCollection services, IConfiguration configuration)
    {
        // Validate JWT settings
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Validate API settings
        services.AddOptions<ApiSettings>()
            .Bind(configuration.GetSection(ApiSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
} 