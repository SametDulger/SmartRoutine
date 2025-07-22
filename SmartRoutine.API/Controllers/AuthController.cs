using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRoutine.Application.Commands.Auth;
using SmartRoutine.Application.DTOs.Auth;
using SmartRoutine.Application.Queries.Auth;
using System.Security.Claims;

namespace SmartRoutine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Geçersiz kullanıcı token.");
        }
        return userId;
    }

    /// <summary>
    /// Yeni kullanıcı kaydı yapar
    /// </summary>
    /// <param name="request">Kayıt bilgileri</param>
    /// <returns>Kayıt edilen kullanıcı bilgileri</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), 201)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequestDto request)
    {
        var command = new RegisterCommand
        {
            Email = request.Email,
            Password = request.Password,
            Role = !string.IsNullOrEmpty(request.Role) ? Enum.Parse<SmartRoutine.Domain.Entities.UserRole>(request.Role, true) : null
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(Register), result);
    }

    /// <summary>
    /// Kullanıcı girişi yapar
    /// </summary>
    /// <param name="request">Giriş bilgileri</param>
    /// <returns>JWT token ve kullanıcı bilgileri</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var command = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Mevcut kullanıcı bilgilerini getirir
    /// </summary>
    /// <returns>Kullanıcı bilgileri</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        try
        {
            var userId = GetCurrentUserId();
            var query = new GetCurrentUserQuery { UserId = userId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcı profilini günceller.
    /// </summary>
    /// <param name="request">Profil güncelleme isteği</param>
    /// <returns>Güncellenmiş kullanıcı bilgisi</returns>
    /// <response code="200">Başarılı güncellenmiş kullanıcı bilgisi</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="404">Kullanıcı bulunamadı</response>
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
    {
        var userId = Guid.Parse(User.Claims.First(x => x.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value);
        var command = new UpdateProfileCommand
        {
            UserId = userId,
            DisplayName = request.DisplayName,
            Email = request.Email,
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Refresh token ile yeni access ve refresh token üretir
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>Yeni access ve refresh token</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokenResponseDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<RefreshTokenResponseDto>> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _mediator.Send(request.RefreshToken);
        return Ok(result);
    }

    /// <summary>
    /// Şifre sıfırlama talebi (kullanıcıya e-posta gönderir)
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        var command = new ForgotPasswordCommand { Email = request.Email };
        var result = await _mediator.Send(command);
        // Güvenlik için her zaman 200 dön
        return Ok(new { message = "Eğer e-posta sistemde kayıtlıysa şifre sıfırlama bağlantısı gönderildi." });
    }

    /// <summary>
    /// E-posta doğrulama (kullanıcı e-posta adresini doğrular)
    /// </summary>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request)
    {
        var command = new VerifyEmailCommand { Email = request.Email, Token = request.Token };
        var result = await _mediator.Send(command);
        if (result)
            return Ok(new { message = "E-posta başarıyla doğrulandı." });
        return BadRequest(new { message = "E-posta doğrulama başarısız veya token geçersiz." });
    }

    /// <summary>
    /// Sadece adminler için örnek endpoint
    /// </summary>
    [HttpGet("admin-only")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminOnly()
    {
        return Ok(new { message = "Bu endpoint sadece adminler içindir." });
    }
} 