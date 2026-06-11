using System.Security.Claims;
using HospitalManagement.BusinessLogic.DTOs.Auth;
using HospitalManagement.BusinessLogic.Services;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
    [EnableRateLimiting("AuthPolicy")]
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
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken ct)
    {
        var userId = await _authService.RegisterAsync(request, ct);
        return CreatedAtAction(nameof(GetCurrentUser), new { }, new { success = true, userId });
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

    /// <summary>Verify an email address.</summary>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request, CancellationToken ct)
    {
        await _authService.VerifyEmailAsync(request, ct);
        return Ok(new { success = true, message = "Email verified successfully." });
    }

    /// <summary>Resend verification email.</summary>
    [HttpPost("resend-verification")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailRequestDto request, CancellationToken ct)
    {
        await _authService.ResendVerificationEmailAsync(request, ct);
        return Ok(new { success = true, message = "Verification email sent." });
    }

    /// <summary>Get login history for the current user.</summary>
    [HttpGet("login-history")]
    [Authorize]
    public async Task<IActionResult> GetMyLoginHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var result = await _authService.GetMyLoginHistoryAsync(userId, pageNumber, pageSize, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get current user profile data.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _authService.GetCurrentUserAsync(userId, ct);
        return Ok(result); // Return the raw object directly as requested: { "userId": "...", "email": "..." }
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

    /// <summary>Request a password reset email.</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request, CancellationToken ct)
    {
        await _authService.ForgotPasswordAsync(request, ct);
        // Always return success to prevent email enumeration
        return Ok(new { success = true, message = "If the email exists, a password reset token has been sent." });
    }

    /// <summary>Reset password using the emailed token.</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordWithTokenRequestDto request, CancellationToken ct)
    {
        await _authService.ResetPasswordWithTokenAsync(request, ct);
        return Ok(new { success = true, message = "Password has been successfully reset." });
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
    
    private Guid GetCurrentUserId()
    {
        var id = User.FindFirstValue(AppConstants.Jwt.ClaimUserId)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User ID claim not found.");
        return Guid.Parse(id);
    }
}
