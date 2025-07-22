namespace SmartRoutine.Application.Interfaces;

public interface INotificationService
{
    Task SendWelcomeEmailAsync(string email, string userName);
    Task SendRoutineReminderAsync(string email, string routineName, TimeOnly scheduledTime);
    Task SendRoutineCompletedEmailAsync(string email, string routineName, int currentStreak);
    Task SendWeeklyStatsEmailAsync(string email, int completedRoutines, double completionRate, int currentStreak);
    Task SendPushNotificationAsync(string userId, string title, string body);
}

public class NotificationRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public Dictionary<string, string>? Data { get; set; }
}

public enum NotificationType
{
    Welcome,
    RoutineReminder,
    RoutineCompleted,
    WeeklyStats,
    Achievement,
    General
} 