using MediatR;
using SmartRoutine.Application.DTOs.Stats;
using SmartRoutine.Application.Common.Interfaces;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.Services;
using SmartRoutine.Domain.Common;

namespace SmartRoutine.Application.Queries.Stats;

public class GetStatsQueryHandler : IRequestHandler<GetStatsQuery, StatsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetStatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StatsDto> Handle(GetStatsQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayStart = today.ToDateTime(TimeOnly.MinValue);
        var todayEnd = today.ToDateTime(TimeOnly.MaxValue);
        var activeRoutines = (await _unitOfWork.Routines.FindAsync(r => r.UserId == request.UserId && r.IsActive, includeProperties: "RoutineLogs")).ToList();
        var totalRoutines = activeRoutines.Count;
        var completedToday = activeRoutines.Count(r => r.RoutineLogs != null && r.RoutineLogs.Any(rl => rl.CompletedAt >= todayStart && rl.CompletedAt <= todayEnd));
        var completionRate = totalRoutines > 0 ? (double)completedToday / totalRoutines * 100 : 0;
        var currentStreak = await CalculateCurrentStreakAsync(request.UserId);
        var bestStreak = currentStreak; // Şimdilik aynı
        var weeklyStats = await GetWeeklyStatsAsync(request.UserId);
        return new StatsDto
        {
            TotalRoutines = totalRoutines,
            CompletedToday = completedToday,
            CompletionRate = Math.Round(completionRate, 2),
            CurrentStreak = currentStreak,
            BestStreak = bestStreak,
            WeeklyStats = weeklyStats
        };
    }

    private async Task<int> CalculateCurrentStreakAsync(Guid userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var streak = 0;
        var routines = (await _unitOfWork.Routines.FindAsync(r => r.UserId == userId && r.IsActive, includeProperties: "RoutineLogs")).ToList();
        if (!routines.Any()) return 0;
        for (int i = 0; i < 365; i++)
        {
            var checkDate = today.AddDays(-i);
            var dayStart = checkDate.ToDateTime(TimeOnly.MinValue);
            var dayEnd = checkDate.ToDateTime(TimeOnly.MaxValue);
            var completedCount = routines.Count(r => r.RoutineLogs != null && r.RoutineLogs.Any(rl => rl.CompletedAt >= dayStart && rl.CompletedAt <= dayEnd));
            if (completedCount > 0)
            {
                streak++;
            }
            else
            {
                break;
            }
        }
        return streak;
    }

    private async Task<Dictionary<string, int>> GetWeeklyStatsAsync(Guid userId)
    {
        var weeklyStats = new Dictionary<string, int>();
        var routines = (await _unitOfWork.Routines.FindAsync(r => r.UserId == userId && r.IsActive, includeProperties: "RoutineLogs")).ToList();
        for (int i = 6; i >= 0; i--)
        {
            var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i));
            var dayStart = date.ToDateTime(TimeOnly.MinValue);
            var dayEnd = date.ToDateTime(TimeOnly.MaxValue);
            var completedCount = routines.Sum(r => r.RoutineLogs != null ? r.RoutineLogs.Count(rl => rl.CompletedAt >= dayStart && rl.CompletedAt <= dayEnd) : 0);
            weeklyStats[date.ToString("yyyy-MM-dd")] = completedCount;
        }
        return weeklyStats;
    }
} 