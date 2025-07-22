using MediatR;
using SmartRoutine.Application.DTOs.Auth;

namespace SmartRoutine.Application.Queries.Auth;

public class GetCurrentUserQuery : IRequest<UserDto>
{
    public Guid UserId { get; set; }
} 