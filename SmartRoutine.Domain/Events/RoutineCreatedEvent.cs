using SmartRoutine.Domain.Common;

namespace SmartRoutine.Domain.Events;

public class RoutineCreatedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid UserId { get; }
    public Guid RoutineId { get; }
    public string Title { get; }
    public string? Description { get; }

    public RoutineCreatedEvent(Guid userId, Guid routineId, string title, string? description)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        UserId = userId;
        RoutineId = routineId;
        Title = title;
        Description = description;
    }
}

public class RoutineCompletedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid UserId { get; }
    public Guid RoutineId { get; }
    public string Title { get; }
    public DateTime CompletedAt { get; }

    public RoutineCompletedEvent(Guid userId, Guid routineId, string title, DateTime completedAt)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        UserId = userId;
        RoutineId = routineId;
        Title = title;
        CompletedAt = completedAt;
    }
}

public class RoutineUpdatedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid UserId { get; }
    public Guid RoutineId { get; }
    public string Title { get; }
    public string? Description { get; }
    public TimeOnly TimeOfDay { get; }

    public RoutineUpdatedEvent(Guid userId, Guid routineId, string title, string? description, TimeOnly timeOfDay)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        UserId = userId;
        RoutineId = routineId;
        Title = title;
        Description = description;
        TimeOfDay = timeOfDay;
    }
}

public class RoutineDeletedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid UserId { get; }
    public Guid RoutineId { get; }
    public string Title { get; }

    public RoutineDeletedEvent(Guid userId, Guid routineId, string title)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        UserId = userId;
        RoutineId = routineId;
        Title = title;
    }
}

public class UserRegisteredEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid UserId { get; }
    public string Email { get; }

    public UserRegisteredEvent(Guid userId, string email)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        UserId = userId;
        Email = email;
    }
}

public class RoutineLogUpdatedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid RoutineLogId { get; }
    public Guid RoutineId { get; }
    public string? Notes { get; }

    public RoutineLogUpdatedEvent(Guid routineLogId, Guid routineId, string? notes)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        RoutineLogId = routineLogId;
        RoutineId = routineId;
        Notes = notes;
    }
} 