using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZuriFluxAPI.DTOs;
using ZuriFluxAPI.Services;

/// <summary>
/// Manages the ZuriFlux waste credit token system
/// Citizens earn credits for proper disposal, lose them for violations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CreditsController : ControllerBase
{
    private readonly ICreditService _creditService;

    public CreditsController(ICreditService creditService)
    {
        _creditService = creditService;
    }

    /// <summary>
    /// Get all credit transactions across the entire system
    /// </summary>
    /// <remarks>Roles allowed: admin only</remarks>
    [HttpGet("transactions")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(PagedResult<CreditTransactionResponseDto>), 200)]
    public async Task<IActionResult> GetAllTransactions([FromQuery] CreditFilterParams filters)
    {
        var transactions = await _creditService.GetPagedAsync(filters);
        return Ok(transactions);
    }                                          // ← this closing brace was missing!

    /// <summary>
    /// Get a user's credit summary — current balance, totals earned and deducted
    /// </summary>
    /// <param name="userId">The user ID to get summary for</param>
    /// <remarks>
    /// Roles allowed: admin (any user), citizen/collector (own account only)
    /// Returns 403 Forbidden if a non-admin tries to view someone else's summary
    /// </remarks>
    [HttpGet("users/{userId}/summary")]
    [ProducesResponseType(typeof(CreditSummaryDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetUserSummary(int userId)
    {
        var requestingUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var requestingRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (requestingRole != "admin" && requestingUserId != userId.ToString())
            return Forbid();

        var summary = await _creditService.GetUserCreditSummaryAsync(userId);
        return Ok(summary);
    }

    /// <summary>
    /// Get full transaction history for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <remarks>
    /// Roles allowed: admin (any user), citizen/collector (own account only)
    /// </remarks>
    [HttpGet("users/{userId}/transactions")]
    [ProducesResponseType(typeof(IEnumerable<CreditTransactionResponseDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetUserTransactions(int userId)
    {
        var requestingUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var requestingRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (requestingRole != "admin" && requestingUserId != userId.ToString())
            return Forbid();

        var transactions = await _creditService.GetUserTransactionsAsync(userId);
        return Ok(transactions);
    }

    /// <summary>
    /// Award credits to a user for proper waste disposal
    /// </summary>
    /// <remarks>
    /// Roles allowed: admin only
    /// Common reasons: "proper disposal", "recycling bonus", "community cleanup"
    /// </remarks>
    [HttpPost("award")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(CreditTransactionResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> AwardCredits([FromBody] AwardCreditDto dto)
    {
        var result = await _creditService.AwardCreditsAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Deduct credits from a user as a penalty
    /// </summary>
    /// <remarks>
    /// Roles allowed: admin only
    /// Common reasons: "illegal dumping penalty", "missed collection"
    /// Returns 400 if user has insufficient balance
    /// </remarks>
    [HttpPost("deduct")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(CreditTransactionResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> DeductCredits([FromBody] DeductCreditDto dto)
    {
        var result = await _creditService.DeductCreditsAsync(dto);
        return Ok(result);
    }
}