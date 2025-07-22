using Microsoft.EntityFrameworkCore;
using SmartRoutine.Application.DTOs.Stats;
using SmartRoutine.Application.Interfaces;
using SmartRoutine.Infrastructure.Data;

namespace SmartRoutine.Infrastructure.Services;

public class StatsService : IStatsService
{
    private readonly ApplicationDbContext _context;

    public StatsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StatsDto> GetSummaryAsync(Guid userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayStart = today.ToDateTime(TimeOnly.MinValue);
        var todayEnd = today.ToDateTime(TimeOnly.MaxValue);

        var activeRoutines = await _context.Routines
            .Where(r => r.UserId == userId && r.IsActive)
            .Include(r => r.RoutineLogs)
            .ToListAsync();

        var totalRoutines = activeRoutines.Count;
        var completedToday = activeRoutines.Count(r => r.RoutineLogs
            .Any(rl => rl.CompletedAt >= todayStart && rl.CompletedAt <= todayEnd));

        var completionRate = totalRoutines > 0 ? (double)completedToday / totalRoutines * 100 : 0;

        var currentStreak = await CalculateCurrentStreakAsync(userId);

        var weeklyStats = await GetWeeklyStatsAsync(userId);

        return new StatsDto
        {
            TotalRoutines = totalRoutines,
            CompletedToday = completedToday,
            CompletionRate = Math.Round(completionRate, 2),
            CurrentStreak = currentStreak,
            BestStreak = currentStreak, // Simplified for now
            WeeklyStats = weeklyStats
        };
    }

    public async Task<int> GetStreakAsync(Guid userId)
    {
        return await CalculateCurrentStreakAsync(userId);
    }

    private async Task<int> CalculateCurrentStreakAsync(Guid userId)
    {
        var routines = await _context.Routines
            .Where(r => r.UserId == userId && r.IsActive)
            .Include(r => r.RoutineLogs)
            .ToListAsync();

        if (!routines.Any())
            return 0;

        var streak = 0;
        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        while (true)
        {
            var dayStart = currentDate.ToDateTime(TimeOnly.MinValue);
            var dayEnd = currentDate.ToDateTime(TimeOnly.MaxValue);

            var completedRoutinesForDay = 0;
            foreach (var routine in routines)
            {
                if (routine.RoutineLogs.Any(rl => rl.CompletedAt >= dayStart && rl.CompletedAt <= dayEnd))
                {
                    completedRoutinesForDay++;
                }
            }

            // If at least 50% of routines were completed, count it as a successful day
            var successThreshold = Math.Ceiling(routines.Count * 0.5);
            if (completedRoutinesForDay >= successThreshold)
            {
                streak++;
                currentDate = currentDate.AddDays(-1);
            }
            else
            {
                break;
            }

            // Prevent infinite loop
            if (currentDate < DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-365)))
                break;
        }

        return streak;
    }

    private async Task<Dictionary<string, int>> GetWeeklyStatsAsync(Guid userId)
    {
        var weekStart = DateTime.UtcNow.AddDays(-7);
        var routines = await _context.Routines
            .Where(r => r.UserId == userId && r.IsActive)
            .Include(r => r.RoutineLogs.Where(rl => rl.CompletedAt >= weekStart))
            .ToListAsync();

        var weeklyStats = new Dictionary<string, int>();

        for (int i = 6; i >= 0; i--)
        {
            var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i));
            var dayStart = date.ToDateTime(TimeOnly.MinValue);
            var dayEnd = date.ToDateTime(TimeOnly.MaxValue);

            var completedCount = routines.Sum(r => r.RoutineLogs.Count(rl => 
                rl.CompletedAt >= dayStart && rl.CompletedAt <= dayEnd));

            weeklyStats[date.ToString("yyyy-MM-dd")] = completedCount;
        }

        return weeklyStats;
    }
} 