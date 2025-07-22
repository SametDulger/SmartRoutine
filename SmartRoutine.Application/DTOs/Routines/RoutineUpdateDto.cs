using System.ComponentModel.DataAnnotations;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.Services;
using SmartRoutine.Domain.Common;

namespace SmartRoutine.Application.DTOs.Routines;

public class RoutineUpdateDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public string TimeOfDay { get; set; } = "08:00";
    
    public string RepeatType { get; set; } = "Daily";
    
    public bool IsActive { get; set; } = true;

    public List<DayOfWeek>? RepeatDays { get; set; } // Haftalık tekrar günleri (opsiyonel)
} 