using MediatR;
using SmartRoutine.Application.Interfaces;
using SmartRoutine.Application.Exceptions;

namespace SmartRoutine.Application.Commands.Auth;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, bool>
{
    private readonly IAuthService _authService;

    public VerifyEmailCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<bool> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Token))
            throw new ValidationException("E-posta ve token zorunludur.");
        return await _authService.VerifyEmailAsync(request.Email, request.Token);
    }
} 