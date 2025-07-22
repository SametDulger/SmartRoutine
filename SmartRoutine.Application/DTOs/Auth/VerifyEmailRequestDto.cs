namespace SmartRoutine.Application.DTOs.Auth;

public class VerifyEmailRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
} 