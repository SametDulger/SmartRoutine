using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRoutine.API.Extensions;
using SmartRoutine.Application.Commands.Routines;
using SmartRoutine.Application.DTOs.Routines;
using SmartRoutine.Application.Queries.Routines;
using System.Security.Claims;
using SmartRoutine.Application.Exceptions;
using SmartRoutine.Domain.Enums;

namespace SmartRoutine.API.Controllers.V1;

/// <summary>
/// Rutin yönetimi endpoints - Version 1
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json")]
public class RoutinesV1Controller : ControllerBase
{
    private readonly IMediator _mediator;

    public RoutinesV1Controller(IMediator mediator)
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
    /// Bugünün rutinlerini getirir
    /// </summary>
    /// <returns>Bugünkü rutinler listesi</returns>
    /// <response code="200">Rutinler başarıyla getirildi</response>
    /// <response code="401">Yetkilendirme hatası</response>
    [HttpGet("today")]
    [ProducesResponseType(typeof(IEnumerable<RoutineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
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
            return Ok(result.Data); // Return only data array for V1 compatibility
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
    /// Sayfalanmış rutin listesi getirir
    /// </summary>
    /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 10)</param>
    /// <param name="onlyActive">Sadece aktif rutinler (varsayılan: true)</param>
    /// <param name="search">Arama terimi</param>
    /// <returns>Sayfalanmış rutin listesi</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoutines(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool onlyActive = true,
        [FromQuery] string? search = null)
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
                SearchTerm = search
            };

            var result = await _mediator.Send(query);

            // Add pagination headers
            Response.AddPaginationHeader(result);

            return Ok(new
            {
                data = result.Data,
                pagination = new
                {
                    totalCount = result.TotalCount,
                    pageNumber = result.PageNumber,
                    pageSize = result.PageSize,
                    totalPages = result.TotalPages,
                    hasPreviousPage = result.HasPreviousPage,
                    hasNextPage = result.HasNextPage
                }
            });
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
    /// Yeni rutin oluşturur
    /// </summary>
    /// <param name="request">Rutin bilgileri</param>
    /// <returns>Oluşturulan rutin</returns>
    [HttpPost]
    [ProducesResponseType(typeof(RoutineDto), StatusCodes.Status201Created)]
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
            return CreatedAtAction(nameof(GetTodayRoutines), new { id = result.Id }, result);
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
    /// <param name="id">Rutin ID</param>
    /// <param name="request">Güncellenecek rutin bilgileri</param>
    /// <returns>Güncellenmiş rutin</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RoutineDto), StatusCodes.Status200OK)]
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
    /// <param name="id">Silinecek rutin ID</param>
    /// <returns>Silme işlemi sonucu</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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
    /// Rutin tamamlar
    /// </summary>
    /// <param name="id">Tamamlanacak rutin ID</param>
    /// <param name="notes">Opsiyonel notlar</param>
    /// <returns>Tamamlama işlemi sonucu</returns>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
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
} 