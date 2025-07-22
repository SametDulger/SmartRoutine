using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace SmartRoutine.Infrastructure.Services;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string toEmail, string userName, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default);
    Task SendRoutineReminderEmailAsync(string toEmail, string routineTitle, TimeOnly scheduledTime, CancellationToken cancellationToken = default);
    Task SendRoutineCompletedEmailAsync(string toEmail, string routineTitle, int currentStreak, CancellationToken cancellationToken = default);
    Task SendEmailVerificationAsync(string email, string verificationToken);
}

public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📧 [MOCK] Welcome email sent to {Email} for user {UserName}", toEmail, userName);
        
        // Simulate email sending delay
        await Task.Delay(100, cancellationToken);
        
        // In production, integrate with:
        // - SendGrid
        // - AWS SES
        // - Azure Communication Services
        // - SMTP server
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📧 [MOCK] Password reset email sent to {Email} with token {Token}", toEmail, resetToken[..8] + "...");
        
        await Task.Delay(100, cancellationToken);
    }

    public async Task SendRoutineReminderEmailAsync(string toEmail, string routineTitle, TimeOnly scheduledTime, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📧 [MOCK] Routine reminder email sent to {Email} for '{RoutineTitle}' at {ScheduledTime}", 
            toEmail, routineTitle, scheduledTime);
        
        await Task.Delay(50, cancellationToken);
    }

    public async Task SendRoutineCompletedEmailAsync(string toEmail, string routineTitle, int currentStreak, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📧 [MOCK] Routine completed email sent to {Email} for '{RoutineTitle}' - Streak: {Streak}", 
            toEmail, routineTitle, currentStreak);
        
        await Task.Delay(50, cancellationToken);
    }

    public Task SendEmailVerificationAsync(string email, string verificationToken)
        => throw new NotImplementedException();
} 

public class SmtpEmailService : IEmailService
{
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly IConfiguration _configuration;

    public SmtpEmailService(ILogger<SmtpEmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName, CancellationToken cancellationToken = default)
    {
        var smtpHost = _configuration["Smtp:Host"];
        var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
        var smtpUser = _configuration["Smtp:User"];
        var smtpPass = _configuration["Smtp:Pass"];
        var fromEmail = _configuration["Smtp:From"] ?? smtpUser;

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };
        var mail = new MailMessage(fromEmail!, toEmail)
        {
            Subject = "SmartRoutine - Hoş Geldiniz!",
            Body = $"Merhaba {userName},\n\nSmartRoutine'a hoş geldiniz!",
            IsBodyHtml = false
        };
        await client.SendMailAsync(mail, cancellationToken);
        _logger.LogInformation("SMTP ile welcome email sent to {Email}", toEmail);
    }

    public Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task SendEmailVerificationAsync(string email, string verificationToken)
        => throw new NotImplementedException();

    public Task SendRoutineReminderEmailAsync(string toEmail, string routineTitle, TimeOnly scheduledTime, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task SendRoutineCompletedEmailAsync(string toEmail, string routineTitle, int currentStreak, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
} 