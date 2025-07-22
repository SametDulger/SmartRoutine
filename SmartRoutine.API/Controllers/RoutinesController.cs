using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRoutine.Application.Commands.Routines;
using SmartRoutine.Application.DTOs.Routines;
using SmartRoutine.Application.Queries.Routines;
using System.Security.Claims;
using SmartRoutine.Application.Exceptions;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Application.Common.Interfaces;

namespace SmartRoutine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoutinesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public RoutinesController(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
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
    /// Bugünkü rutinleri getirir
    /// </summary>
    [HttpGet("today")]
    public async Task<IActionResult> GetTodayRoutines()
    {
        try
        {
            var userId = GetCurrentUserId();
            var query = new GetUserRoutinesQuery
            {
                UserId = userId,
                PageNumber = 1,
                PageSize = 50, // Get all today routines
                OnlyActive = true
            };

            var result = await _mediator.Send(query);
            return Ok(result.Data); // Return only data array for backward compatibility
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Haftalık rutinleri getirir
    /// </summary>
    [HttpGet("week")]
    public async Task<IActionResult> GetWeekRoutines()
    {
        try
        {
            var userId = GetCurrentUserId();
            var query = new GetUserRoutinesQuery
            {
                UserId = userId,
                PageNumber = 1,
                PageSize = 100, // Get all week routines
                OnlyActive = true
            };

            var result = await _mediator.Send(query);
            return Ok(result.Data); // Return only data array for backward compatibility
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcıya ait rutinleri listeler.
    /// </summary>
    /// <returns>Kullanıcının rutinleri</returns>
    /// <response code="200">Başarılı rutin listesi</response>
    [ProducesResponseType(typeof(IEnumerable<RoutineDto>), 200)]
    [HttpGet]
    public async Task<IActionResult> GetUserRoutines(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool onlyActive = true,
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var query = new GetUserRoutinesQuery
            {
                UserId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                OnlyActive = onlyActive,
                SearchTerm = searchTerm
            };

            var result = await _mediator.Send(query);
            return Ok(result.Data); // Return only data array for consistency
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Yeni rutin oluşturur
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateRoutine([FromBody] RoutineCreateDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new CreateRoutineCommand
            {
                UserId = userId,
                Title = request.Title,
                Description = request.Description,
                TimeOfDay = request.TimeOfDay,
                RepeatType = request.RepeatType, // string olarak ata
                RepeatDays = request.RepeatDays
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetUserRoutines), new { id = result.Id }, result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return StatusCode(409, new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Rutin günceller
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoutine(Guid id, [FromBody] RoutineUpdateDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new UpdateRoutineCommand
            {
                UserId = userId,
                RoutineId = id,
                Title = request.Title,
                Description = request.Description,
                TimeOfDay = request.TimeOfDay,
                RepeatType = request.RepeatType, // string olarak ata
                IsActive = request.IsActive
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return StatusCode(409, new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Rutin siler
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoutine(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new DeleteRoutineCommand
            {
                UserId = userId,
                RoutineId = id
            };

            var result = await _mediator.Send(command);
            
            if (!result)
            {
                return NotFound(new { message = "Rutin bulunamadı." });
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Rutin tamamlar
    /// </summary>
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteRoutine(Guid id, [FromBody] string? notes = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new CompleteRoutineCommand
            {
                UserId = userId,
                RoutineId = id,
                Notes = notes
            };

            var result = await _mediator.Send(command);
            
            if (!result)
            {
                return BadRequest(new { message = "Rutin tamamlanamadı. Rutin bulunamadı veya zaten tamamlandı." });
            }

            return Ok(new { message = "Rutin başarıyla tamamlandı." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Debug endpoint - Tüm rutinleri listeler (sadece development'ta)
    /// </summary>
    [HttpGet("debug/all")]
    public async Task<IActionResult> GetAllRoutinesDebug()
    {
        try
        {
            var userId = GetCurrentUserId();
            var routines = (await _unitOfWork.Routines.FindAsync(r => r.UserId == userId, includeProperties: "RoutineLogs")).ToList();
            
            var result = routines.Select(r => new
            {
                Id = r.Id,
                Title = r.Title.Value,
                IsActive = r.IsActive,
                IsDeleted = r.IsDeleted,
                CreatedAt = r.CreatedAt,
                RoutineLogsCount = r.RoutineLogs?.Count ?? 0
            }).ToList();
            
            return Ok(new { 
                userId = userId,
                totalRoutines = routines.Count,
                routines = result 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Tüm kullanıcıların rutinlerini sadece adminler görebilir
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAllRoutinesForAdmin()
    {
        // Sadece örnek: Gerçek implementasyon için servis katmanında tüm rutinler çekilmeli
        return Ok(new { message = "Bu endpoint sadece adminler içindir." });
    }
} 