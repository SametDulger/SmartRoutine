using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.Events;
using SmartRoutine.Domain.Services;
using SmartRoutine.Domain.Common;

namespace SmartRoutine.Application.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(User user);
    Guid? GetUserIdFromToken(string token);
    bool ValidateToken(string token);
    string GenerateRefreshToken();
    Task AddRefreshTokenToBlacklistAsync(string refreshToken);
    Task<bool> IsRefreshTokenBlacklistedAsync(string refreshToken);
    Task RemoveRefreshTokenFromBlacklistAsync(string refreshToken);
    Task ClearRefreshTokenBlacklistAsync();
} 