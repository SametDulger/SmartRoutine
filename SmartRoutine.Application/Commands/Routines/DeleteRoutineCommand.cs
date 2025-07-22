using MediatR;

namespace SmartRoutine.Application.Commands.Routines;

public class DeleteRoutineCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public Guid RoutineId { get; set; }
} 