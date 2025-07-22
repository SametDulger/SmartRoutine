using MediatR;
using SmartRoutine.Application.DTOs.Routines;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.Services;
using SmartRoutine.Domain.Common;

namespace SmartRoutine.Application.Commands.Routines;

public class UpdateRoutineCommand : IRequest<RoutineDto>
{
    public Guid UserId { get; set; }
    public Guid RoutineId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TimeOfDay { get; set; } = "08:00";
    public string RepeatType { get; set; } = "Daily";
    public bool IsActive { get; set; }
    public List<DayOfWeek>? RepeatDays { get; set; }
} 