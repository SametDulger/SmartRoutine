using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRoutine.Application.Queries.Stats;
using System.Security.Claims;

namespace SmartRoutine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatsController(IMediator mediator)
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
    /// Kullanıcının genel istatistiklerini getirir
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            var userId = GetCurrentUserId();
            var query = new GetStatsQuery { UserId = userId };
            var stats = await _mediator.Send(query);
            return Ok(stats);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcının mevcut streak bilgisini getirir
    /// </summary>
    [HttpGet("streak")]
    public async Task<IActionResult> GetStreak()
    {
        try
        {
            var userId = GetCurrentUserId();
            var query = new GetStreakQuery { UserId = userId };
            var streak = await _mediator.Send(query);
            return Ok(new { streak });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
} 