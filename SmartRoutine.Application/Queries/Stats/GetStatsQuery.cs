using MediatR;
using SmartRoutine.Application.Behaviors;
using SmartRoutine.Application.DTOs.Stats;

namespace SmartRoutine.Application.Queries.Stats;

public class GetStatsQuery : IRequest<StatsDto>, ICacheable
{
    public Guid UserId { get; set; }

    public string CacheKey => $"stats-{UserId}";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(15);
    public TimeSpan? AbsoluteExpiration => null;
} 