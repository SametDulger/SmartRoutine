using Microsoft.Extensions.Logging;
using SmartRoutine.Application.Interfaces;
using SmartRoutine.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;

namespace SmartRoutine.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IEmailService emailService, ILogger<NotificationService> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string email, string userName)
    {
        try
        {
            await _emailService.SendWelcomeEmailAsync(email, userName);
            _logger.LogInformation("Welcome email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", email);
            throw;
        }
    }

    public async Task SendRoutineReminderAsync(string email, string routineName, TimeOnly scheduledTime)
    {
        try
        {
            await _emailService.SendRoutineReminderEmailAsync(email, routineName, scheduledTime);
            _logger.LogInformation("Routine reminder sent to {Email} for routine {RoutineName}", email, routineName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send routine reminder to {Email}", email);
            throw;
        }
    }

    public async Task SendRoutineCompletedEmailAsync(string email, string routineName, int currentStreak)
    {
        try
        {
            await _emailService.SendRoutineCompletedEmailAsync(email, routineName, currentStreak);
            _logger.LogInformation("Routine completed email sent to {Email} for routine {RoutineName}", email, routineName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send routine completed email to {Email}", email);
            throw;
        }
    }

    public async Task SendWeeklyStatsEmailAsync(string email, int completedRoutines, double completionRate, int currentStreak)
    {
        try
        {
            // Enhanced weekly stats email with more detailed information
            _logger.LogInformation("Preparing weekly stats email for {Email}", email);
            
            var emailContent = GenerateWeeklyStatsContent(completedRoutines, completionRate, currentStreak);
            
            // In a real implementation, you would use a templating engine
            // like Razor Pages, Handlebars, or similar to generate HTML content
            
            // For now, we'll simulate sending the weekly stats email
            await Task.Delay(150); // Simulate email processing time
            
            _logger.LogInformation("Weekly stats email sent to {Email} - Completed: {Completed}, Rate: {Rate}%, Streak: {Streak}", 
                email, completedRoutines, completionRate, currentStreak);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send weekly stats email to {Email}", email);
            throw;
        }
    }

    public async Task SendPushNotificationAsync(string userId, string title, string message, Dictionary<string, string>? data = null)
    {
        try
        {
            // Enhanced push notification implementation
            var notificationPayload = new
            {
                to = userId,
                title = title,
                body = message,
                data = data ?? new Dictionary<string, string>(),
                timestamp = DateTime.UtcNow,
                priority = "high"
            };

            _logger.LogInformation("Preparing push notification for user {UserId}: {Title}", userId, title);
            
            // In a real implementation, this would integrate with:
            // - Firebase Cloud Messaging (FCM)
            // - Apple Push Notification Service (APNS)
            // - Azure Notification Hubs
            // - Amazon Simple Notification Service (SNS)
            
            await Task.Delay(100); // Simulate network call
            
            _logger.LogInformation("Push notification sent successfully to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification to user {UserId}", userId);
            throw;
        }
    }

    public Task SendPushNotificationAsync(string userId, string title, string body)
        => SendPushNotificationAsync(userId, title, body, null);

    public async Task SendPasswordResetEmailAsync(string email, string resetToken)
    {
        _logger.LogInformation($"[MOCK] Password reset email sent to {email} with token: {resetToken}");
        await Task.CompletedTask;
    }

    public async Task SendEmailVerificationAsync(string email, string verificationToken)
    {
        _logger.LogInformation($"[MOCK] Email verification sent to {email} with token: {verificationToken}");
        await Task.CompletedTask;
    }

    private string GenerateWeeklyStatsContent(int completedRoutines, double completionRate, int currentStreak)
    {
        // In a production environment, this would use a proper templating system
        return $@"
            <h2>Haftalık Rutin Özeti</h2>
            <p>Bu hafta harika bir performans sergileydiniz!</p>
            <ul>
                <li>Tamamlanan Rutinler: {completedRoutines}</li>
                <li>Başarı Oranı: %{completionRate:F1}</li>
                <li>Mevcut Seri: {currentStreak} gün</li>
            </ul>
            <p>Devam edin, harika gidiyorsunuz! 🎉</p>
        ";
    }
} 

public class FcmPushNotificationService : INotificationService
{
    private readonly ILogger<FcmPushNotificationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public FcmPushNotificationService(ILogger<FcmPushNotificationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = new HttpClient();
    }

    public async Task SendWelcomeEmailAsync(string email, string userName)
    {
        // FCM ile e-posta gönderilmez, boş bırak
        await Task.CompletedTask;
    }

    public async Task SendRoutineReminderAsync(string email, string routineName, TimeOnly scheduledTime)
    {
        // FCM ile e-posta gönderilmez, boş bırak
        await Task.CompletedTask;
    }

    public async Task SendRoutineCompletedEmailAsync(string email, string routineName, int currentStreak)
    {
        // FCM ile e-posta gönderilmez, boş bırak
        await Task.CompletedTask;
    }

    public async Task SendWeeklyStatsEmailAsync(string email, int completedRoutines, double completionRate, int currentStreak)
    {
        // FCM ile e-posta gönderilmez, boş bırak
        await Task.CompletedTask;
    }

    public async Task SendPushNotificationAsync(string userId, string title, string message, Dictionary<string, string>? data = null)
    {
        var fcmServerKey = _configuration["Fcm:ServerKey"];
        var fcmEndpoint = "https://fcm.googleapis.com/fcm/send";
        var payload = new
        {
            to = userId,
            notification = new { title, body = message },
            data = data ?? new Dictionary<string, string>()
        };
        var request = new HttpRequestMessage(HttpMethod.Post, fcmEndpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation("Authorization", $"key={fcmServerKey}");
        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("FCM push notification sent to {UserId}", userId);
        }
        else
        {
            _logger.LogError("FCM push notification failed: {Status}", response.StatusCode);
        }
    }

    public Task SendPushNotificationAsync(string userId, string title, string body)
        => SendPushNotificationAsync(userId, title, body, null);
} 