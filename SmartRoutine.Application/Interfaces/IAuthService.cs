using SmartRoutine.Application.DTOs.Auth;

namespace SmartRoutine.Application.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Kullanıcıyı giriş yapar.
    /// </summary>
    /// <param name="email">Kullanıcı e-posta adresi</param>
    /// <param name="password">Kullanıcı şifresi</param>
    /// <returns>LoginResponseDto</returns>
    Task<LoginResponseDto> LoginAsync(string email, string password);
    /// <summary>
    /// Yeni kullanıcı kaydı oluşturur.
    /// </summary>
    /// <param name="request">Kayıt isteği</param>
    /// <returns>Kullanıcı DTO</returns>
    Task<UserDto> RegisterAsync(RegisterRequestDto request);
    Task<UserDto> GetCurrentUserAsync(Guid userId);
    Task<bool> ValidateTokenAsync(string token);
    Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken);
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> VerifyEmailAsync(string email, string token);
    /// <summary>
    /// Kullanıcı profilini günceller.
    /// </summary>
    /// <param name="userId">Kullanıcı Id</param>
    /// <param name="displayName">Ad Soyad</param>
    /// <param name="email">E-posta</param>
    /// <param name="currentPassword">Mevcut şifre</param>
    /// <param name="newPassword">Yeni şifre</param>
    /// <returns>Güncellenmiş kullanıcı DTO</returns>
    Task<UserDto?> UpdateProfileAsync(Guid userId, string? displayName, string? email, string? currentPassword, string? newPassword);
} 