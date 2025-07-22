using System.ComponentModel.DataAnnotations;
using SmartRoutine.Domain.Common;

namespace SmartRoutine.Domain.Entities;

public class RoutineLog : BaseEntity
{
    [Required]
    public Guid RoutineId { get; private set; }
    
    public DateTime CompletedAt { get; private set; } = DateTime.UtcNow;
    
    public string? Notes { get; private set; }
    
    public bool IsDeleted { get; private set; } = false;
    public DateTime? DeletedAt { get; private set; }
    
    // Navigation property
    public virtual Routine Routine { get; private set; } = null!;

    private RoutineLog() { } // EF Core constructor

    public RoutineLog(Guid routineId, string? notes = null) : base()
    {
        RoutineId = routineId;
        Notes = notes;
        CompletedAt = DateTime.UtcNow;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        SetUpdatedAt();
        AddDomainEvent(new Domain.Events.RoutineLogUpdatedEvent(Id, RoutineId, notes));
    }

    public void MarkAsDeleted()
    {
        if (!IsDeleted)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            SetUpdatedAt();
        }
    }
} 