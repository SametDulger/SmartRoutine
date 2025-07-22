namespace SmartRoutine.Domain.Events;

public class RoutineUpdatedIntegrationEvent : IntegrationEvent
{
    public Guid RoutineId { get; set; }
    public Guid UserId { get; set; }
    public string? Title { get; set; }
} 