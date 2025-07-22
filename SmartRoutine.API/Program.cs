using Serilog;
using SmartRoutine.API.BackgroundServices;
using SmartRoutine.API.Extensions;
using SmartRoutine.API.Filters;
using SmartRoutine.API.Middleware;
using SmartRoutine.API.Validators;
using SmartRoutine.Infrastructure.Data;
using Microsoft.AspNetCore.ResponseCompression;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/smartroutine-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog
builder.Host.UseSerilog();

// Add configuration validation
builder.Services.AddConfigurationValidation(builder.Configuration);

// Add services using extension methods
builder.Services.AddControllers();
builder.Services.AddApiVersioningSupport();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCorsPolicy();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddHealthChecks(builder.Configuration);
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

// Add filters
builder.Services.AddScoped<ApiKeyAuthFilter>();

// Add background services
builder.Services.AddHostedService<RoutineReminderService>();
builder.Services.AddSignalR();
Microsoft.Extensions.DependencyInjection.LocalizationServiceCollectionExtensions.AddLocalization(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartRoutine API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
    });
}

// Add rate limiting middleware - temporarily disabled for development
// app.UseMiddleware<RateLimitingMiddleware>();

// Add global exception handling middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Add health checks
app.UseHealthChecks();

// Apply database migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.InitializeAsync(context, app.Environment.IsDevelopment());
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<SmartRoutine.API.Hubs.NotificationHub>("/hubs/notification")
    .RequireCors("AllowAll");

app.UseResponseCompression();

try
{
    Log.Information("SmartRoutine API başlatılıyor...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API başlatılamadı");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for integration tests
public partial class Program { } 