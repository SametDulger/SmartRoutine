using Microsoft.EntityFrameworkCore;
using SmartRoutine.Application.DTOs.Routines;
using SmartRoutine.Application.Interfaces;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Infrastructure.Data;
using SmartRoutine.Application.Exceptions;
using SmartRoutine.Domain.ValueObjects;

namespace SmartRoutine.Infrastructure.Services;

public class RoutineService : IRoutineService
{
    private readonly ApplicationDbContext _context;
    private readonly IDomainEventService _domainEventService;

    public RoutineService(ApplicationDbContext context, IDomainEventService domainEventService)
    {
        _context = context;
        _domainEventService = domainEventService;
    }

    public async Task<IEnumerable<RoutineDto>> GetTodayRoutinesAsync(Guid userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var routines = await _context.Routines
            .Where(r => r.UserId == userId && r.IsActive)
            .Include(r => r.RoutineLogs)
            .ToListAsync();

        return routines.Select(r => MapToDto(r, today)).ToList();
    }

    public async Task<IEnumerable<RoutineDto>> GetWeekRoutinesAsync(Guid userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var routines = await _context.Routines
            .Where(r => r.UserId == userId && r.IsActive)
            .Include(r => r.RoutineLogs)
            .ToListAsync();

        return routines.Select(r => MapToDto(r, today)).ToList();
    }

    public async Task<RoutineDto> CreateRoutineAsync(Guid userId, RoutineCreateDto request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var routine = new Routine(
                userId,
                request.Title,
                request.Description,
                Enum.Parse<RoutineRepeatType>(request.RepeatType),
                request.RepeatDays,
                null);

            _context.Routines.Add(routine);
            await _context.SaveChangesAsync();
            // Domain event publish
            await _domainEventService.PublishAsync(routine.DomainEvents);
            routine.ClearDomainEvents();
            await transaction.CommitAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return MapToDto(routine, today);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<RoutineDto> UpdateRoutineAsync(Guid userId, Guid routineId, RoutineUpdateDto request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var routine = await _context.Routines.FirstOrDefaultAsync(r => r.Id == routineId && r.UserId == userId);
            
            if (routine == null)
            {
                throw new NotFoundException("Error_RoutineNotFound");
            }

            routine.Update(request.Title, request.Description, Enum.Parse<RoutineRepeatType>(request.RepeatType), request.RepeatDays, null);
            
            if (request.IsActive)
                routine.Activate();
            else
                routine.Deactivate();

            await _context.SaveChangesAsync();
            // Domain event publish
            await _domainEventService.PublishAsync(routine.DomainEvents);
            routine.ClearDomainEvents();
            await transaction.CommitAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return MapToDto(routine, today);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteRoutineAsync(Guid userId, Guid routineId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var routine = await _context.Routines.FirstOrDefaultAsync(r => r.Id == routineId && r.UserId == userId);
            
            if (routine == null)
            {
                return false;
            }

            // Soft delete uygula
            routine.MarkAsDeleted();
            await _context.SaveChangesAsync();
            // Domain event publish
            await _domainEventService.PublishAsync(routine.DomainEvents);
            routine.ClearDomainEvents();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> CompleteRoutineAsync(Guid userId, Guid routineId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var routine = await _context.Routines
                .Include(r => r.RoutineLogs)
                .FirstOrDefaultAsync(r => r.Id == routineId && r.UserId == userId && r.IsActive);
            
            if (routine == null)
            {
                return false;
            }

            try
            {
                var log = routine.CompleteToday(); // This uses domain logic and raises events
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (ValidationException)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static RoutineDto MapToDto(Routine routine, DateOnly today)
    {
        var todayStart = today.ToDateTime(TimeOnly.MinValue);
        var todayEnd = today.ToDateTime(TimeOnly.MaxValue);

        var isCompletedToday = routine.RoutineLogs
            .Any(rl => rl.CompletedAt >= todayStart && rl.CompletedAt <= todayEnd);

        return new RoutineDto
        {
            Id = routine.Id,
            Title = routine.Title,
            Description = routine.Description,
            // TimeOfDay kaldırıldı
            RepeatType = routine.RepeatType,
            IsActive = routine.IsActive,
            IsCompletedToday = isCompletedToday,
            CreatedAt = routine.CreatedAt,
            UpdatedAt = routine.UpdatedAt
        };
    }
} 