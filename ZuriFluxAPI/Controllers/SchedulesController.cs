using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Services;

/// <summary>
/// Manages waste collection scheduling — citizens book pickup slots
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public SchedulesController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    /// <summary>
    /// Get all schedules with filtering and pagination
    /// </summary>
    /// <remarks>
    /// Roles allowed: admin (all), collector (own), citizen (own)
    /// Query params: status, timeSlot, fromDate, toDate, collectorId, userId
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ScheduleResponseDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] ScheduleFilterParams filters)
    {
        var requestingUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var requestingRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Non-admins can only see their own schedules
        if (requestingRole == "citizen")
            filters.UserId = int.Parse(requestingUserId);
        else if (requestingRole == "collector")
            filters.CollectorId = int.Parse(requestingUserId);

        var result = await _scheduleService.GetPagedAsync(filters);
        return Ok(result);
    }

    /// <summary>
    /// Get a single schedule by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ScheduleResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var schedule = await _scheduleService.GetByIdAsync(id);
        if (schedule == null) return NotFound($"Schedule with ID {id} not found.");
        return Ok(schedule);
    }

    /// <summary>
    /// Book a new pickup schedule for a bin
    /// </summary>
    /// <remarks>
    /// Roles allowed: any authenticated user
    /// TimeSlot options: morning, afternoon, evening
    /// Cannot book in the past or double-book a bin slot
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ScheduleResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleDto dto)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var result = await _scheduleService.CreateScheduleAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Assign a collector to a schedule
    /// </summary>
    /// <remarks>Roles allowed: admin only</remarks>
    [HttpPatch("{id}/assign")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(ScheduleResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AssignCollector(
        int id, [FromBody] AssignCollectorDto dto)
    {
        var result = await _scheduleService.AssignCollectorAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Update the status of a schedule
    /// </summary>
    /// <remarks>
    /// Roles allowed: admin, collector
    /// Valid statuses: accepted, completed, cancelled
    /// Completing a schedule resets the bin and awards collector 15 credits
    /// </remarks>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "admin,collector")]
    [ProducesResponseType(typeof(ScheduleResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateStatus(
        int id, [FromBody] UpdateScheduleStatusDto dto)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var result = await _scheduleService.UpdateStatusAsync(id, dto, userId);
        return Ok(result);
    }
}