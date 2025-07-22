using MediatR;

namespace SmartRoutine.Application.Commands.Auth;

public class VerifyEmailCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
} 