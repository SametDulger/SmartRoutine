using MediatR;
using SmartRoutine.Application.Common.Interfaces;
using SmartRoutine.Application.Exceptions;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.Services;
using SmartRoutine.Domain.Common;

namespace SmartRoutine.Application.Commands.Routines;

public class CompleteRoutineCommandHandler : IRequestHandler<CompleteRoutineCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRoutineValidationService _validationService;
    private readonly ICacheService _cacheService;

    public CompleteRoutineCommandHandler(
        IUnitOfWork unitOfWork,
        IRoutineValidationService validationService,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _cacheService = cacheService;
    }

    public async Task<bool> Handle(CompleteRoutineCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
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

            // Validate if routine can be completed today
            if (!_validationService.CanCompleteRoutineToday(routine))
            {
                throw new ValidationException("Bu rutin bugün tamamlanamaz. Rutin aktif değil veya zaten tamamlandı.");
            }

            // Complete routine using domain method (raises domain event)
            var routineLog = routine.CompleteToday();

            // Explicitly add the routine log to the repository
            await _unitOfWork.RoutineLogs.AddAsync(routineLog);

            // Save changes (this will dispatch domain events)
            var savedCount = await _unitOfWork.SaveChangesAsync(cancellationToken);
            // Cache invalidation: kullanıcının rutin ve stats cache'ini temizle
            await _cacheService.RemoveByPatternAsync($"routines-{request.UserId}", cancellationToken);
            await _cacheService.RemoveByPatternAsync($"stats-{request.UserId}", cancellationToken);
            await _cacheService.RemoveByPatternAsync($"streak-{request.UserId}", cancellationToken);
            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch (InvalidOperationException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw new ValidationException(ex.Message);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
} 