using SmartRoutine.Domain.Entities;

namespace SmartRoutine.Domain.Services;

public interface IRoutineValidationService
{
    bool IsValidTimeSlot(TimeOnly timeOfDay, Guid userId, Guid? excludeRoutineId = null);
    bool CanCompleteRoutineToday(Routine routine);
    bool IsRoutineActiveForToday(Routine routine);
    int GetMaxRoutinesPerDay();
    bool IsValidRoutineSchedule(Routine routine, DateTime targetDate);
    bool CanCreateMoreRoutines(Guid userId, IEnumerable<Routine> userRoutines);
}

public class RoutineValidationService : IRoutineValidationService
{
    public bool IsValidTimeSlot(TimeOnly timeOfDay, Guid userId, Guid? excludeRoutineId = null)
    {
        // Business rule: Time slots must be in 15-minute increments
        if (timeOfDay.Minute % 15 != 0)
        {
            return false;
        }

        // Business rule: Users can schedule routines between 5:00 AM and 11:00 PM
        var earliestTime = new TimeOnly(5, 0);  // 5:00 AM
        var latestTime = new TimeOnly(23, 0);   // 11:00 PM

        if (timeOfDay < earliestTime || timeOfDay > latestTime)
        {
            return false;
        }

        // In a real implementation, you would check for time slot conflicts
        // This would require access to repository to check existing routines
        return true;
    }

    public bool CanCompleteRoutineToday(Routine routine)
    {
        if (!routine.IsActive)
        {
            return false;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayStart = today.ToDateTime(TimeOnly.MinValue);
        var todayEnd = today.ToDateTime(TimeOnly.MaxValue);

        // Check if routine is already completed today
        var alreadyCompleted = routine.RoutineLogs
            .Any(log => log.CompletedAt >= todayStart && log.CompletedAt <= todayEnd);

        if (alreadyCompleted)
        {
            return false;
        }

        // Check if routine is scheduled for today based on repeat type
        return IsRoutineActiveForToday(routine);
    }

    public bool IsRoutineActiveForToday(Routine routine)
    {
        if (!routine.IsActive) 
            return false;

        var today = DateTime.UtcNow;
        switch (routine.RepeatType)
        {
            case Domain.Enums.RoutineRepeatType.Daily:
                return true;
            case Domain.Enums.RoutineRepeatType.Weekly:
                return IsValidWeeklySchedule(routine, today);
            case Domain.Enums.RoutineRepeatType.CustomDays:
                if (routine.CustomDays == null || routine.CustomDays.Count == 0)
                    return false;
                return routine.CustomDays.Contains(today.DayOfWeek);
            case Domain.Enums.RoutineRepeatType.IntervalBased:
                if (!routine.IntervalDays.HasValue)
                    return false;
                // Varsayım: Routine oluşturulma tarihi başlangıçtır
                var daysSinceCreated = (today.Date - routine.CreatedAt.Date).Days;
                return daysSinceCreated % routine.IntervalDays.Value == 0;
            default:
                return false;
        }
    }

    public bool IsValidRoutineSchedule(Routine routine, DateTime targetDate)
    {
        switch (routine.RepeatType)
        {
            case Domain.Enums.RoutineRepeatType.Daily:
                return true;
            case Domain.Enums.RoutineRepeatType.Weekly:
                return IsValidWeeklySchedule(routine, targetDate);
            case Domain.Enums.RoutineRepeatType.CustomDays:
                if (routine.CustomDays == null || routine.CustomDays.Count == 0)
                    return false;
                return routine.CustomDays.Contains(targetDate.DayOfWeek);
            case Domain.Enums.RoutineRepeatType.IntervalBased:
                if (!routine.IntervalDays.HasValue)
                    return false;
                var daysSinceCreated = (targetDate.Date - routine.CreatedAt.Date).Days;
                return daysSinceCreated % routine.IntervalDays.Value == 0;
            default:
                return false;
        }
    }

    public bool CanCreateMoreRoutines(Guid userId, IEnumerable<Routine> userRoutines)
    {
        var activeRoutineCount = userRoutines.Count(r => r.IsActive);
        return activeRoutineCount < GetMaxRoutinesPerDay();
    }

    public int GetMaxRoutinesPerDay()
    {
        // Business rule: Maximum routines per day
        return 20;
    }

    private bool IsValidWeeklySchedule(Routine routine, DateTime targetDate)
    {
        // For weekly routines, we could add more complex logic here
        // For now, we assume weekly routines are active every day
        // In a more complex implementation, you could have specific days of week
        return true;
    }
} 