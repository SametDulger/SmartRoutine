using MediatR;
using SmartRoutine.Application.Common.Interfaces;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.Services;
using SmartRoutine.Domain.Common;

namespace SmartRoutine.Application.Queries.Stats;

public class GetStreakQueryHandler : IRequestHandler<GetStreakQuery, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetStreakQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(GetStreakQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var streak = 0;
        var routines = (await _unitOfWork.Routines.FindAsync(r => r.UserId == request.UserId && r.IsActive)).ToList();
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
} 