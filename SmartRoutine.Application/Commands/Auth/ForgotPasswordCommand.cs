using MediatR;

namespace SmartRoutine.Application.Commands.Auth;

public class ForgotPasswordCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;
} 