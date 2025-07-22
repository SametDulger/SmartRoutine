using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartRoutine.Application.Behaviors;
using SmartRoutine.Application.Common.Interfaces;
using SmartRoutine.Application.Interfaces;
using SmartRoutine.Application.Mappings;
using SmartRoutine.Application.Validators;
using SmartRoutine.Domain.Services;
using SmartRoutine.Infrastructure.Common;
using SmartRoutine.Infrastructure.Data;
using SmartRoutine.Infrastructure.Repositories;
using SmartRoutine.Infrastructure.Services;
using System.Reflection;
using System.Text;
#if DEBUG
using Microsoft.Data.Sqlite;
#endif
using StackExchange.Redis;

namespace SmartRoutine.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));
        
        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(RegisterRequestValidator).Assembly);
        
        // MediatR - Register handlers from both Application and Infrastructure
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssemblies(
                typeof(SmartRoutine.Application.Interfaces.IAuthService).Assembly,
                typeof(SmartRoutine.Domain.Entities.User).Assembly,
                typeof(SmartRoutine.Infrastructure.Services.AuthService).Assembly);

            // Add pipeline behaviors
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        });
        
        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database - Environment specific
        var isTestEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test" ||
                                Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Test";
        
        if (isTestEnvironment)
        {
#if DEBUG
            // Use SQLite in-memory database for tests with persistent connection
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            services.AddSingleton(connection);
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(connection);
                options.EnableSensitiveDataLogging();
            });
#else
            // Use SQLite in-memory database for tests
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite("DataSource=:memory:");
                options.EnableSensitiveDataLogging();
            });
#endif
        }
        else
        {
            // Use SQLite for production
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
        }

        // Repositories - using Infrastructure IRepository
        services.AddScoped(typeof(SmartRoutine.Infrastructure.Repositories.IRepository<>), typeof(Repository<>));

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoutineService, RoutineService>();
        services.AddScoped<IStatsService, StatsService>();
        services.AddScoped<ITokenService, TokenService>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var cache = provider.GetRequiredService<ICacheService>();
            return new TokenService(config, cache);
        });
        services.AddScoped<ILoggingService, LoggingService>();
        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddScoped<INotificationService, NotificationService>();
        
        // Domain Services
        services.AddScoped<IRoutineValidationService, RoutineValidationService>();
        
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Cache Services
        var redisConnectionString = configuration["Redis:ConnectionString"];
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
            services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            services.AddScoped<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddMemoryCache();
            services.AddScoped<ICacheService, InMemoryCacheService>();
        }

        // Email Services
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? configuration["ASPNETCORE_ENVIRONMENT"] ?? configuration["Environment"] ?? "Production";
        var isDevelopment = env.ToLowerInvariant() == "development";
        if (isDevelopment)
        {
            services.AddScoped<IEmailService, MockEmailService>();
        }
        else
        {
            var smtpPassword = GetSmtpPassword(configuration);
            services.AddScoped<IEmailService, SmtpEmailService>();
        }

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSecretKey = GetJwtSecretKey(configuration);
        var key = Encoding.ASCII.GetBytes(jwtSecretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }

    private static string GetJwtSecretKey(IConfiguration configuration)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
        var jwtSecretKey = Environment.GetEnvironmentVariable("SMARTROUTINE_JWT_SECRET") ?? configuration["Jwt:SecretKey"];
        if (string.IsNullOrEmpty(jwtSecretKey) && env.ToLowerInvariant() == "production")
            throw new ArgumentNullException("JWT SecretKey is required as environment variable (SMARTROUTINE_JWT_SECRET) in production.");
        return jwtSecretKey ?? string.Empty;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "SmartRoutine API",
                Description = "Akıllı rutin takip uygulaması API'si"
            });

            // JWT Authentication için Swagger yapılandırması
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });

            // XML yorumlarını dahil et
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder
                    .WithOrigins("http://localhost:5002", "https://localhost:5002", "http://192.168.1.100:5000", "exp://192.168.1.100:8081")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static IServiceCollection AddLocalization(this IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddControllers()
            .AddViewLocalization()
            .AddDataAnnotationsLocalization();
        return services;
    }

    private static string GetSmtpPassword(IConfiguration configuration)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
        var smtpPassword = Environment.GetEnvironmentVariable("SMARTROUTINE_SMTP_PASSWORD");
        if (string.IsNullOrEmpty(smtpPassword) && env.ToLowerInvariant() == "production")
            throw new ArgumentNullException("SMTP password is required as environment variable (SMARTROUTINE_SMTP_PASSWORD) in production.");
        return smtpPassword ?? string.Empty;
    }
} 