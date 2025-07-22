using Microsoft.EntityFrameworkCore;
using SmartRoutine.Infrastructure.Data;
using SmartRoutine.Infrastructure.Services;

namespace SmartRoutine.API.BackgroundServices;

public class RoutineReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RoutineReminderService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromMinutes(15); // Check every 15 minutes

    public RoutineReminderService(IServiceProvider serviceProvider, ILogger<RoutineReminderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_period);
        
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await CheckRoutineRemindersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking routine reminders");
            }
        }
    }

    private async Task CheckRoutineRemindersAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var loggingService = scope.ServiceProvider.GetRequiredService<ILoggingService>();

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var todayStart = today.ToDateTime(TimeOnly.MinValue);
        var todayEnd = today.ToDateTime(TimeOnly.MaxValue);

        // TimeOfDay ile ilgili tüm erişimler kaldırıldı
        // Rutin hatırlatma mantığı sadeleştirildi, sadece aktif ve tamamlanmamış rutinler loglanacak
        var routinesNeedingReminder = await context.Routines
            .Include(r => r.User)
            .Include(r => r.RoutineLogs)
            .Where(r => r.IsActive)
            .Where(r => !r.RoutineLogs.Any(rl => rl.CompletedAt >= todayStart && rl.CompletedAt <= todayEnd))
            .ToListAsync();

        foreach (var routine in routinesNeedingReminder)
        {
            _logger.LogInformation("Routine reminder: {RoutineTitle} for user {UserEmail}", 
                routine.Title, routine.User.Email);

            // Burada notification servisi entegre edilebilir
            loggingService.LogRoutineCompleted(routine.UserId, routine.Id, 
                $"Reminder: {routine.Title}");
        }

        if (routinesNeedingReminder.Any())
        {
            _logger.LogInformation("Processed {Count} routine reminders", routinesNeedingReminder.Count);
        }
    }
} 