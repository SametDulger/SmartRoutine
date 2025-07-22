using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.Services;
using SmartRoutine.Domain.Common;

namespace SmartRoutine.Application.DTOs.Routines;

public class RoutineDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TimeOnly TimeOfDay { get; set; }
    public RoutineRepeatType RepeatType { get; set; }
    public bool IsActive { get; set; }
    public bool IsCompletedToday { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 