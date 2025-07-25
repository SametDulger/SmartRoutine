using System.ComponentModel.DataAnnotations;

namespace SmartRoutine.Application.DTOs.Auth;

public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
} 