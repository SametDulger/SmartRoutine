using AutoMapper;
using MediatR;
using SmartRoutine.Application.DTOs.Auth;
using SmartRoutine.Application.Interfaces;
using SmartRoutine.Application.Common.Interfaces;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.Services;
using SmartRoutine.Domain.Common;
using SmartRoutine.Domain.ValueObjects;
using BCrypt.Net;

namespace SmartRoutine.Application.Commands.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public LoginCommandHandler(IUnitOfWork unitOfWork, ITokenService tokenService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var user = (await _unitOfWork.Users.FindAsync(u => u.Email == email.Value)).FirstOrDefault();
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Geçersiz email veya şifre.");
        }
        var token = _tokenService.GenerateJwtToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        return new LoginResponseDto
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RefreshToken = refreshToken,
            User = _mapper.Map<UserDto>(user)
        };
    }
} 