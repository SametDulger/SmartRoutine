using MediatR;
using SmartRoutine.Application.Behaviors;

namespace SmartRoutine.Application.Queries.Stats;

public class GetStreakQuery : IRequest<int>, ICacheable
{
    public Guid UserId { get; set; }

    public string CacheKey => $"streak-{UserId}";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(10);
    public TimeSpan? AbsoluteExpiration => null;
} 