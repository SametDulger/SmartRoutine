using AutoMapper;
using MediatR;
using SmartRoutine.Application.Common.Interfaces;
using SmartRoutine.Application.DTOs.Routines;
using SmartRoutine.Application.Exceptions;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.Services;
using SmartRoutine.Domain.Common;
using SmartRoutine.Domain.ValueObjects;

namespace SmartRoutine.Application.Commands.Routines;

public class UpdateRoutineCommandHandler : IRequestHandler<UpdateRoutineCommand, RoutineDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRoutineValidationService _validationService;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public UpdateRoutineCommandHandler(
        IUnitOfWork unitOfWork,
        IRoutineValidationService validationService,
        IMapper mapper,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<RoutineDto> Handle(UpdateRoutineCommand request, CancellationToken cancellationToken)
    {
        // Get the routine
        var routine = await _unitOfWork.Routines.GetByIdAsync(request.RoutineId);
        
        if (routine == null)
        {
            throw new NotFoundException($"Rutin bulunamadı: {request.RoutineId}");
        }

        // Verify ownership
        if (routine.UserId != request.UserId)
        {
            throw new UnauthorizedException("Error_RoutineAccessDenied");
        }

        // routine.TimeOfDay ile ilgili tüm erişimler ve kontroller kaldırıldı

        // Update routine using domain method (raises domain event)
        routine.Update(
            request.Title,
            request.Description,
            Enum.Parse<RoutineRepeatType>(request.RepeatType),
            request.RepeatDays,
            null
        );
        
        // Handle activation/deactivation
        if (request.IsActive && !routine.IsActive)
        {
            routine.Activate();
        }
        else if (!request.IsActive && routine.IsActive)
        {
            routine.Deactivate();
        }

        // Save changes (this will dispatch domain events)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache invalidation: kullanıcının rutin ve stats cache'ini temizle
        await _cacheService.RemoveByPatternAsync($"routines-{request.UserId}", cancellationToken);
        await _cacheService.RemoveByPatternAsync($"stats-{request.UserId}", cancellationToken);
        await _cacheService.RemoveByPatternAsync($"streak-{request.UserId}", cancellationToken);

        // Map to DTO
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayStart = today.ToDateTime(TimeOnly.MinValue);
        var todayEnd = today.ToDateTime(TimeOnly.MaxValue);

        var dto = _mapper.Map<RoutineDto>(routine);
        dto.IsCompletedToday = routine.RoutineLogs
            .Any(rl => rl.CompletedAt >= todayStart && rl.CompletedAt <= todayEnd);

        return dto;
    }
} 