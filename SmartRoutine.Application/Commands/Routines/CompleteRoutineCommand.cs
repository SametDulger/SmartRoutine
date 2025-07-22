using MediatR;

namespace SmartRoutine.Application.Commands.Routines;

public class CompleteRoutineCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public Guid RoutineId { get; set; }
    public string? Notes { get; set; }
} 