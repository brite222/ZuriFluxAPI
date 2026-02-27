using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Services;

/// <summary>
/// Manages IoT waste bins and sensor readings across Lagos
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BinsController : ControllerBase
{
    private readonly IBinService _binService;

    public BinsController(IBinService binService)
    {
        _binService = binService;
    }

    /// <summary>
    /// Get all bins in the system
    /// </summary>
    /// <remarks>Roles allowed: any</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BinResponseDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] BinFilterParams filters)
    {
        var result = await _binService.GetPagedBinsAsync(filters);
        return Ok(result);
    }

    /// <summary>
    /// Get a single bin by ID including its sensor reading history
    /// </summary>
    /// <param name="id">The bin ID</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BinResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var bin = await _binService.GetBinByIdAsync(id);
        if (bin == null) return NotFound($"Bin with ID {id} not found.");
        return Ok(bin);
    }

    /// <summary>
    /// Get all bins currently flagged as needing pickup (fill level 80%+)
    /// </summary>
    /// <remarks>
    /// Roles allowed: any
    /// Use this endpoint to power the collector alert dashboard
    /// </remarks>
    [HttpGet("alerts")]
    [ProducesResponseType(typeof(IEnumerable<BinResponseDto>), 200)]
    public async Task<IActionResult> GetBinsNeedingPickup()
    {
        var bins = await _binService.GetBinsNeedingPickupAsync();
        return Ok(bins);
    }

    /// <summary>
    /// Register a new IoT bin in the system
    /// </summary>
    /// <remarks>Roles allowed: admin only</remarks>
    [HttpPost]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(BinResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Create([FromBody] CreateBinDto dto)
    {
        var created = await _binService.CreateBinAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Receive a sensor reading from an IoT bin
    /// </summary>
    /// <remarks>
    /// Roles allowed: any
    /// Called automatically by IoT hardware — not by users directly.
    /// Automatically flags bin for pickup if fill level reaches 80% or above.
    /// </remarks>
    [HttpPost("readings")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> PostSensorReading([FromBody] SensorReadingDto dto)
    {
        await _binService.ProcessSensorReadingAsync(dto);
        return Ok(new { message = "Reading processed successfully." });
    }
}