using System.ComponentModel.DataAnnotations;

namespace SmartRoutine.Application.DTOs.Auth;

public class UpdateProfileRequestDto
{
    [StringLength(100)]
    public string? DisplayName { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
    
    public string? CurrentPassword { get; set; }
    
    [MinLength(6)]
    public string? NewPassword { get; set; }
} 