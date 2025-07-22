using MediatR;
using SmartRoutine.Application.Interfaces;
using SmartRoutine.Application.Exceptions;

namespace SmartRoutine.Application.Commands.Auth;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IAuthService _authService;

    public ForgotPasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("E-posta adresi zorunludur.");
        return await _authService.ForgotPasswordAsync(request.Email);
    }
} 