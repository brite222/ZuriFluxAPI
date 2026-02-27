using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Services;

/// <summary>
/// Provides system-wide analytics for government and management dashboards
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Get a full system overview — bins, collections, users, credits
    /// </summary>
    /// <remarks>
    /// Roles allowed: admin only
    /// Primary endpoint for the government dashboard homepage
    /// </remarks>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardSummaryDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetDashboard()
    {
        var summary = await _analyticsService.GetDashboardSummaryAsync();
        return Ok(summary);
    }

    /// <summary>
    /// Get bin health breakdown and hotspot locations across Lagos
    /// </summary>
    /// <remarks>
    /// Roles allowed: admin only
    /// Returns full bins, moderate bins, empty bins + top 5 busiest locations
    /// </remarks>
    [HttpGet("bins")]
    [ProducesResponseType(typeof(BinAnalyticsDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetBinAnalytics()
    {
        var analytics = await _analyticsService.GetBinAnalyticsAsync();
        return Ok(analytics);
    }

    /// <summary>
    /// Get waste collection performance stats and top collectors leaderboard
    /// </summary>
    /// <remarks>Roles allowed: admin only</remarks>
    [HttpGet("collections")]
    [ProducesResponseType(typeof(CollectionAnalyticsDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetCollectionAnalytics()
    {
        var analytics = await _analyticsService.GetCollectionAnalyticsAsync();
        return Ok(analytics);
    }

    /// <summary>
    /// Get credit system overview — circulation, top earning citizens
    /// </summary>
    /// <remarks>Roles allowed: admin only</remarks>
    [HttpGet("credits")]
    [ProducesResponseType(typeof(CreditAnalyticsDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetCreditAnalytics()
    {
        var analytics = await _analyticsService.GetCreditAnalyticsAsync();
        return Ok(analytics);
    }
}