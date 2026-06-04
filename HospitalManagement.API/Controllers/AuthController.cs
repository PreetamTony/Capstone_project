using System.Security.Claims;
using HospitalManagement.BusinessLogic.DTOs.Auth;
using HospitalManagement.BusinessLogic.Services;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

/// <summary>Authentication endpoints — login, register, change password.</summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>Login and receive a JWT token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Register a new patient account.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken ct)
    {
        var userId = await _authService.RegisterAsync(request, ct);
        return CreatedAtAction(nameof(Login), new { success = true, userId });
    }

    /// <summary>Change the current user's password.</summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _authService.ChangePasswordAsync(userId, request, ct);
        return NoContent();
    }

    /// <summary>Refresh an expired JWT token.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(request, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Revoke a refresh token (Logout).</summary>
    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequestDto request, CancellationToken ct)
    {
        await _authService.RevokeTokenAsync(request, ct);
        return NoContent();
    }

    /// <summary>Deactivate a user account (Admin only).</summary>
    [HttpPost("deactivate/{userId:guid}")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Deactivate(Guid userId, CancellationToken ct)
    {
        await _authService.DeactivateUserAsync(userId, ct);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var id = User.FindFirstValue(AppConstants.Jwt.ClaimUserId)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User ID claim not found.");
        return Guid.Parse(id);
    }
}
