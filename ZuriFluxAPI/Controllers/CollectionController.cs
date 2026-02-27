using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Services;

/// <summary>
/// Manages waste collection events — logging pickups and tracking history
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CollectionsController : ControllerBase
{
    private readonly ICollectionService _collectionService;

    public CollectionsController(ICollectionService collectionService)
    {
        _collectionService = collectionService;
    }

    /// <summary>
    /// Get all waste collections across the entire system
    /// </summary>
    /// <remarks>Roles allowed: admin only</remarks>
    [HttpGet]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(PagedResult<CollectionResponseDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] CollectionFilterParams filters)
    {
        var collections = await _collectionService.GetPagedAsync(filters);
        return Ok(collections);
    }

    /// <summary>
    /// Get full collection history for a specific bin
    /// </summary>
    /// <param name="binId">The bin ID to get history for</param>
    /// <remarks>Roles allowed: admin only</remarks>
    [HttpGet("bin/{binId}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(IEnumerable<CollectionResponseDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetByBin(int binId)
    {
        var collections = await _collectionService.GetByBinIdAsync(binId);
        return Ok(collections);
    }

    /// <summary>
    /// Get all collections logged by a specific collector
    /// </summary>
    /// <param name="collectorId">The collector's user ID</param>
    /// <remarks>Roles allowed: admin, collector</remarks>
    [HttpGet("collector/{collectorId}")]
    [Authorize(Roles = "admin,collector")]
    [ProducesResponseType(typeof(IEnumerable<CollectionResponseDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetByCollector(int collectorId)
    {
        var collections = await _collectionService.GetByCollectorIdAsync(collectorId);
        return Ok(collections);
    }

    /// <summary>
    /// Log a completed waste pickup
    /// </summary>
    /// <remarks>
    /// Roles allowed: admin, collector
    /// Automatically resets the bin's fill level to 0 and awards the collector 10 credits.
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "admin,collector")]
    [ProducesResponseType(typeof(CollectionResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> LogCollection([FromBody] CreateCollectionDto dto)
    {
        var result = await _collectionService.LogCollectionAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Update the status of a collection record
    /// </summary>
    /// <param name="id">The collection ID</param>
    /// <remarks>
    /// Roles allowed: admin only
    /// Valid statuses: completed, failed, pending
    /// </remarks>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(CollectionResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateCollectionStatusDto dto)
    {
        var result = await _collectionService.UpdateStatusAsync(id, dto);
        return Ok(result);
    }
}