using SmartRoutine.Application.DTOs.Routines;

namespace SmartRoutine.Application.Interfaces;

public interface IRoutineService
{
    Task<IEnumerable<RoutineDto>> GetTodayRoutinesAsync(Guid userId);
    Task<IEnumerable<RoutineDto>> GetWeekRoutinesAsync(Guid userId);
    Task<RoutineDto> CreateRoutineAsync(Guid userId, RoutineCreateDto request);
    Task<RoutineDto> UpdateRoutineAsync(Guid userId, Guid routineId, RoutineUpdateDto request);
    Task<bool> DeleteRoutineAsync(Guid userId, Guid routineId);
    Task<bool> CompleteRoutineAsync(Guid userId, Guid routineId);
} 