using MediatR;
using SmartRoutine.Application.DTOs.Auth;

namespace SmartRoutine.Application.Commands.Auth;

public class UpdateProfileCommand : IRequest<UserDto>
{
    public Guid UserId { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
} 