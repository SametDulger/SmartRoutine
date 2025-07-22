using AutoMapper;
using MediatR;
using SmartRoutine.Application.DTOs.Common;
using SmartRoutine.Application.DTOs.Routines;
using SmartRoutine.Application.Common.Interfaces;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.Services;
using SmartRoutine.Domain.Common;

namespace SmartRoutine.Application.Queries.Routines;

public class GetUserRoutinesQueryHandler : IRequestHandler<GetUserRoutinesQuery, PagedResult<RoutineDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUserRoutinesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<RoutineDto>> Handle(GetUserRoutinesQuery request, CancellationToken cancellationToken)
    {
        var routines = (await _unitOfWork.Routines.FindAsync(r => r.UserId == request.UserId, includeProperties: "RoutineLogs")).ToList();
        
        if (request.OnlyActive)
        {
            routines = routines.Where(r => r.IsActive).ToList();
        }
        
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            routines = routines.Where(r => r.Title.Value.Contains(request.SearchTerm) ||
                (!string.IsNullOrEmpty(r.Description) && r.Description.Contains(request.SearchTerm))).ToList();
        }
        
        var totalCount = routines.Count;
        routines = routines
            .OrderBy(r => r.Title.Value)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayStart = today.ToDateTime(TimeOnly.MinValue);
        var todayEnd = today.ToDateTime(TimeOnly.MaxValue);
        
        var routineDtos = routines.Select(r =>
        {
            var dto = _mapper.Map<RoutineDto>(r);
            dto.IsCompletedToday = r.RoutineLogs != null && r.RoutineLogs.Any(rl => rl.CompletedAt >= todayStart && rl.CompletedAt <= todayEnd);
            return dto;
        }).ToList();
        
        return new PagedResult<RoutineDto>(routineDtos, totalCount, request.PageNumber, request.PageSize);
    }
} 