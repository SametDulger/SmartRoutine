using System.ComponentModel.DataAnnotations;
using SmartRoutine.Domain.Common;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.ValueObjects;
#if TEST
using System.ComponentModel.DataAnnotations.Schema;
#endif
#if DEBUG
using System.ComponentModel.DataAnnotations.Schema;
#endif

namespace SmartRoutine.Domain.Entities;

public class Routine : BaseEntity
{
    [Required]
    public Guid UserId { get; private set; }
    
    public RoutineTitle Title { get; private set; } = null!;
    
    [StringLength(1000)]
    public string? Description { get; private set; }
    
    public RoutineRepeatType RepeatType { get; private set; } = RoutineRepeatType.Daily;
    
    public bool IsActive { get; private set; } = true;
    
    public bool IsDeleted { get; private set; } = false;
    public DateTime? DeletedAt { get; private set; }
    
    public List<DayOfWeek>? CustomDays { get; private set; } // CustomDays için
    public int? IntervalDays { get; private set; } // IntervalBased için
    public TimeOnly TimeOfDay { get; private set; } = new TimeOnly(8, 0);
    
    // Navigation properties
    public virtual User User { get; private set; } = null!;
    public virtual ICollection<RoutineLog> RoutineLogs { get; private set; } = new List<RoutineLog>();

    private Routine() { } // EF Core constructor

    public Routine(Guid userId, string title, string? description, RoutineRepeatType repeatType, List<DayOfWeek>? customDays = null, int? intervalDays = null, TimeOnly? timeOfDay = null) : base()
    {
        UserId = userId;
        Title = RoutineTitle.Create(title);
        Description = description;
        RepeatType = repeatType;
        CustomDays = customDays;
        IntervalDays = intervalDays;
        IsActive = true; // Routines are active by default
        TimeOfDay = timeOfDay ?? new TimeOnly(8, 0);
        
        // Raise domain event
        AddDomainEvent(new RoutineCreatedEvent(UserId, Id, Title, description));
    }

    public void Update(string title, string? description, RoutineRepeatType repeatType, List<DayOfWeek>? customDays = null, int? intervalDays = null)
    {
        Title = RoutineTitle.Create(title);
        Description = description;
        RepeatType = repeatType;
        CustomDays = customDays;
        IntervalDays = intervalDays;
        SetUpdatedAt();
        
        // Raise domain event
        AddDomainEvent(new RoutineUpdatedEvent(UserId, Id, Title, description, TimeOnly.MinValue));
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            SetUpdatedAt();
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            SetUpdatedAt();
        }
    }

    public void MarkAsDeleted()
    {
        if (!IsDeleted)
        {
            AddDomainEvent(new RoutineDeletedEvent(UserId, Id, Title));
            IsActive = false;
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            SetUpdatedAt();
        }
    }

    public RoutineLog CompleteToday()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayStart = today.ToDateTime(TimeOnly.MinValue);
        var todayEnd = today.ToDateTime(TimeOnly.MaxValue);

        if (RoutineLogs.Any(rl => rl.CompletedAt >= todayStart && rl.CompletedAt <= todayEnd))
        {
            throw new InvalidOperationException("Rutin bugün zaten tamamlandı.");
        }

        var log = new RoutineLog(Id, "Tamamlandı");
        RoutineLogs.Add(log);
        AddDomainEvent(new RoutineCompletedEvent(UserId, Id, Title, log.CompletedAt));
        SetUpdatedAt();
        return log;
    }
} 