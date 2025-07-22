using SmartRoutine.Application.Interfaces;

namespace SmartRoutine.Infrastructure.Services;

public class FcmNotificationService : INotificationService
{
    public async Task SendPushNotificationAsync(string userId, string title, string body)
    {
        // FCM entegrasyonu burada olacak (örnek)
        await Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string email, string userName)
        => throw new NotImplementedException();

    public Task SendRoutineReminderAsync(string email, string routineName, TimeOnly scheduledTime)
        => throw new NotImplementedException();

    public Task SendRoutineCompletedEmailAsync(string email, string routineName, int currentStreak)
        => throw new NotImplementedException();

    public Task SendWeeklyStatsEmailAsync(string email, int completedRoutines, double completionRate, int currentStreak)
        => throw new NotImplementedException();
} 