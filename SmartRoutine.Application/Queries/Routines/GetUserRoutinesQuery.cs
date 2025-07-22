using MediatR;
using SmartRoutine.Application.DTOs.Common;
using SmartRoutine.Application.DTOs.Routines;

namespace SmartRoutine.Application.Queries.Routines;

public class GetUserRoutinesQuery : IRequest<PagedResult<RoutineDto>>
{
    public Guid UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public bool OnlyActive { get; set; } = true;
    public string? SearchTerm { get; set; }
} 