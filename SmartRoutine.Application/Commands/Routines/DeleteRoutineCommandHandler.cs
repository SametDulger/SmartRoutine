using MediatR;
using SmartRoutine.Application.Common.Interfaces;
using SmartRoutine.Application.Exceptions;

namespace SmartRoutine.Application.Commands.Routines;

public class DeleteRoutineCommandHandler : IRequestHandler<DeleteRoutineCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public DeleteRoutineCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<bool> Handle(DeleteRoutineCommand request, CancellationToken cancellationToken)
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

        // Mark as deleted using domain method (raises domain event)
        routine.MarkAsDeleted();

        // Save changes (this will dispatch domain events)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache invalidation: kullanıcının rutin ve stats cache'ini temizle
        await _cacheService.RemoveByPatternAsync($"routines-{request.UserId}", cancellationToken);
        await _cacheService.RemoveByPatternAsync($"stats-{request.UserId}", cancellationToken);
        await _cacheService.RemoveByPatternAsync($"streak-{request.UserId}", cancellationToken);

        return true;
    }
} 