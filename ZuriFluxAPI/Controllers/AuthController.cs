using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZuriFluxAPI.Services;

/// <summary>
/// Handles user registration and authentication
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <remarks>
    /// Roles: citizen, collector, admin
    /// Returns a JWT token on success — store this for all future requests
    /// </remarks>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <remarks>
    /// Returns a JWT token — send this in the Authorization header as: Bearer YOUR_TOKEN
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Get the currently logged in user's profile
    /// </summary>
    /// <remarks>
    /// Roles allowed: any authenticated user
    /// </remarks>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMe()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return Unauthorized();
        var user = await _authService.GetUserByIdAsync(int.Parse(userIdClaim));
        if (user == null) return NotFound();
        return Ok(user);
    }
}