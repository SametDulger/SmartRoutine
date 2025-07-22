using SmartRoutine.Application.DTOs.Stats;

namespace SmartRoutine.Application.Interfaces;

public interface IStatsService
{
    Task<StatsDto> GetSummaryAsync(Guid userId);
    Task<int> GetStreakAsync(Guid userId);
} 