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

public class CreateRoutineCommandHandler : IRequestHandler<CreateRoutineCommand, RoutineDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRoutineValidationService _validationService;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public CreateRoutineCommandHandler(
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

    public async Task<RoutineDto> Handle(CreateRoutineCommand request, CancellationToken cancellationToken)
    {
        // Kullanıcı başına günlük maksimum rutin sayısı
        var userRoutines = (await _unitOfWork.Routines.FindAsync(r => r.UserId == request.UserId && r.IsActive && !r.IsDeleted)).ToList();
        if (userRoutines.Count >= 20)
        {
            throw new ValidationException("Bir kullanıcı en fazla 20 aktif rutin oluşturabilir.");
        }

        // Aynı saatte birden fazla aktif rutin kontrolü kaldırıldı
        // Silinmiş rutin aynı başlık ve saatle tekrar oluşturulamaz kontrolü kaldırıldı
        // Domain validation kaldırıldı

        // Create routine using domain constructor (raises domain event)
        var routine = new Routine(
            request.UserId,
            request.Title,
            request.Description,
            Enum.Parse<RoutineRepeatType>(request.RepeatType),
            request.RepeatDays,
            null,
            TimeOnly.Parse(request.TimeOfDay)
        );

        // Add to repository
        await _unitOfWork.Routines.AddAsync(routine);

        // Save changes (this will dispatch domain events)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache invalidation: kullanıcının rutin ve stats cache'ini temizle
        await _cacheService.RemoveByPatternAsync($"routines-{request.UserId}", cancellationToken);
        await _cacheService.RemoveByPatternAsync($"stats-{request.UserId}", cancellationToken);
        await _cacheService.RemoveByPatternAsync($"streak-{request.UserId}", cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayStart = today.ToDateTime(TimeOnly.MinValue);
        var todayEnd = today.ToDateTime(TimeOnly.MaxValue);

        var dto = _mapper.Map<RoutineDto>(routine);
        dto.IsCompletedToday = routine.RoutineLogs
            .Any(rl => rl.CompletedAt >= todayStart && rl.CompletedAt <= todayEnd);

        return dto;
    }
} 