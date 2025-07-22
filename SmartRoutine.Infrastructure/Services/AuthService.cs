using Microsoft.EntityFrameworkCore;
using SmartRoutine.Application.DTOs.Auth;
using SmartRoutine.Application.Interfaces;
using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.ValueObjects;
using SmartRoutine.Infrastructure.Data;
using SmartRoutine.Application.Exceptions;
using SmartRoutine.Application.Common.Interfaces;

namespace SmartRoutine.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IDomainEventService _domainEventService;
    private readonly IEmailService _emailService;

    public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService, IDomainEventService domainEventService, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _domainEventService = domainEventService;
        _emailService = emailService;
    }

    public async Task<UserDto> RegisterAsync(RegisterRequestDto request)
    {
        var email = Email.Create(request.Email);
        if ((await _unitOfWork.Users.FindAsync(u => u.Email == email.Value)).Any())
        {
            throw new ValidationException("Error_EmailAlreadyInUse");
        }
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User(email, passwordHash, UserRole.User, request.DisplayName);
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            await _domainEventService.PublishAsync(user.DomainEvents);
            user.ClearDomainEvents();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email.Value,
            DisplayName = user.DisplayName,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var email = Email.Create(request.Email);
        var user = (await _unitOfWork.Users.FindAsync(u => u.Email == email.Value)).FirstOrDefault();
        if (user == null)
        {
            throw new NotFoundException("Error_UserNotFound");
        }
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Error_InvalidEmailOrPassword");
        }
        var token = _tokenService.GenerateJwtToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken(user.Id, refreshToken, DateTime.UtcNow.AddDays(14), null);
        user.RefreshTokens.Add(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();
        return new LoginResponseDto
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email.Value,
                DisplayName = user.DisplayName,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public Task<LoginResponseDto> LoginAsync(string email, string password)
        => throw new NotImplementedException();

    public async Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken)
    {
        // Blacklist kontrolü
        if (await _tokenService.IsRefreshTokenBlacklistedAsync(refreshToken))
            throw new UnauthorizedException("Refresh token geçersiz veya iptal edilmiş.");

        // Refresh token'ı bul
        var refreshTokenEntity = (await _unitOfWork.Users.GetAllAsync())
            .SelectMany(u => u.RefreshTokens)
            .FirstOrDefault(rt => rt.Token == refreshToken && rt.IsActive);
        if (refreshTokenEntity == null || refreshTokenEntity.IsExpired)
            throw new UnauthorizedException("Refresh token geçersiz veya süresi dolmuş.");

        // Kullanıcıyı bul
        var user = await _unitOfWork.Users.GetByIdAsync(refreshTokenEntity.UserId);
        if (user == null)
            throw new UnauthorizedException("Kullanıcı bulunamadı.");

        // Eski refresh token'ı blacklist'e ekle
        await _tokenService.AddRefreshTokenToBlacklistAsync(refreshToken);
        refreshTokenEntity.Revoke(null);

        // Yeni refresh token üret
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newRefreshTokenEntity = new RefreshToken(user.Id, newRefreshToken, DateTime.UtcNow.AddDays(14), null);
        user.RefreshTokens.Add(newRefreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        // Yeni access token üret
        var accessToken = _tokenService.GenerateJwtToken(user);
        return new RefreshTokenResponseDto
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RefreshToken = newRefreshToken
        };
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("Error_UserNotFound");
        }
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email.Value,
            DisplayName = user.DisplayName,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        return await Task.FromResult(_tokenService.ValidateToken(token));
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = (await _unitOfWork.Users.FindAsync(u => u.Email == email)).FirstOrDefault();
        if (user == null)
            return false; // Güvenlik için kullanıcı yoksa da true dön
        var resetToken = _tokenService.GenerateRefreshToken();
        // Burada token'ı DB'ye kaydedebilir veya kısa süreli bir alan ekleyebilirsin
        // E-posta gönder
        await _emailService.SendPasswordResetEmailAsync(user.Email.Value, resetToken);
        return true;
    }

    public async Task<bool> VerifyEmailAsync(string email, string token)
    {
        var user = (await _unitOfWork.Users.FindAsync(u => u.Email == email)).FirstOrDefault();
        if (user == null)
            return false;
        // Token kontrolü (örnek: user.EmailVerificationToken ile karşılaştırılabilir)
        // Burada basit bir örnek, gerçek projede token'ı DB'de saklamalısın
        // if (user.EmailVerificationToken != token)
        //     return false;
        // user.EmailVerified = true;
        // await _unitOfWork.SaveChangesAsync();
        // return true;
        // Şimdilik her zaman false dön
        return false;
    }

    public async Task<UserDto?> UpdateProfileAsync(Guid userId, string? displayName, string? email, string? currentPassword, string? newPassword)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return null;
        if (!string.IsNullOrWhiteSpace(displayName))
            user.DisplayName = displayName;
        if (!string.IsNullOrWhiteSpace(email) && user.Email.Value != email)
        {
            // E-posta değişikliği için ek kontrol yapılabilir
            user.UpdateEmail(Email.Create(email));
        }
        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            if (string.IsNullOrWhiteSpace(currentPassword) || !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                return null;
            user.UpdatePassword(BCrypt.Net.BCrypt.HashPassword(newPassword));
        }
        await _unitOfWork.SaveChangesAsync();
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email.Value,
            DisplayName = user.DisplayName,
            CreatedAt = user.CreatedAt
        };
    }
} 