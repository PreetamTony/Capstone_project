using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using HospitalManagement.BusinessLogic.DTOs.Auth;
using HospitalManagement.DataAccess.Constants;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace HospitalManagement.BusinessLogic.Services;

/// <summary>Handles user authentication and JWT token generation.</summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _configuration;

    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnitOfWork uow, IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _uow = uow;
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        var user = await _uow.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim(), ct)
            ?? throw new NotFoundException("User", request.Email);

        if (!user.IsActive)
            throw new BusinessRuleViolationException("AccountDeactivated", "This account has been deactivated.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new BusinessRuleViolationException("InvalidCredentials", "Invalid email or password.");

        user.LastLoginAt = DateTime.UtcNow;
        _uow.Users.Update(user);
        await _uow.CompleteAsync(ct);

        var (token, expiresAt) = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        var rtEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days expiry
            IsRevoked = false
        };
        await _uow.RefreshTokens.AddAsync(rtEntity, ct);
        await _uow.CompleteAsync(ct);

        // Resolve full name
        var fullName = await ResolveFullNameAsync(user, ct);

        _logger.LogInformation("User {Email} logged in successfully", user.Email);

        return new LoginResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            FullName = fullName
        };
    }

    /// <inheritdoc/>
    public async Task<Guid> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default)
    {
        // Check email uniqueness
        if (await _uow.Users.AnyAsync(u => u.Email == request.Email.ToLower().Trim(), ct))
            throw new BusinessRuleViolationException("DuplicateEmail", $"Email '{request.Email}' is already registered.");

        await _uow.BeginTransactionAsync(ct);
        try
        {
            var user = new User
            {
                Email = request.Email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                PhoneNumber = request.PhoneNumber,
                Role = UserRole.Patient,
                IsActive = true
            };

            await _uow.Users.AddAsync(user, ct);

            var patient = new Patient
            {
                UserId = user.Id,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Address = request.Address,
                EmergencyContactName = request.EmergencyContactName,
                EmergencyContactPhone = request.EmergencyContactPhone,
                InsuranceProvider = request.InsuranceProvider,
                InsurancePolicyNumber = request.InsurancePolicyNumber,
                InsuranceCoveragePercent = request.InsuranceCoveragePercent
            };

            await _uow.Patients.AddAsync(patient, ct);
            await _uow.CommitTransactionAsync(ct);


            _logger.LogInformation("New patient registered: {Email}, UserId: {UserId}", user.Email, user.Id);
            return user.Id;
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken ct = default)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
            throw new BusinessRuleViolationException("PasswordMismatch", "New password and confirmation do not match.");

        var user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new BusinessRuleViolationException("InvalidPassword", "Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        _uow.Users.Update(user);
        await _uow.CompleteAsync(ct);


        _logger.LogInformation("User {UserId} changed their password", userId);
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken ct = default)
    {
        var principal = GetPrincipalFromExpiredToken(request.Token);
        if (principal == null)
            throw new BusinessRuleViolationException("InvalidToken", "Invalid access token.");

        var idClaim = principal.FindFirstValue(AppConstants.Jwt.ClaimUserId) ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idClaim == null || !Guid.TryParse(idClaim, out var userId))
            throw new BusinessRuleViolationException("InvalidToken", "Invalid user ID in token.");

        var user = await _uow.Users.GetByIdAsync(userId, ct) ?? throw new NotFoundException("User", userId);

        var existingToken = await _uow.RefreshTokens.Query()
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userId, ct);

        if (existingToken == null || !existingToken.IsActive)
            throw new BusinessRuleViolationException("InvalidToken", "Invalid or expired refresh token.");

        // Revoke the old token and generate a new one
        existingToken.IsRevoked = true;
        existingToken.RevokedAt = DateTime.UtcNow;
        _uow.RefreshTokens.Update(existingToken);

        var (newToken, newExpiresAt) = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();

        var rtEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await _uow.RefreshTokens.AddAsync(rtEntity, ct);
        await _uow.CompleteAsync(ct);

        return new LoginResponseDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = newExpiresAt,
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            FullName = await ResolveFullNameAsync(user, ct)
        };
    }

    public async Task RevokeTokenAsync(RevokeTokenRequestDto request, CancellationToken ct = default)
    {
        var existingToken = await _uow.RefreshTokens.Query()
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, ct);

        if (existingToken == null || !existingToken.IsActive)
            throw new BusinessRuleViolationException("InvalidToken", "Invalid or already revoked token.");

        existingToken.IsRevoked = true;
        existingToken.RevokedAt = DateTime.UtcNow;
        _uow.RefreshTokens.Update(existingToken);
        await _uow.CompleteAsync(ct);
    }

    /// <inheritdoc/>
    public async Task DeactivateUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);

        user.IsActive = false;
        _uow.Users.Update(user);
        await _uow.CompleteAsync(ct);


        _logger.LogWarning("User {UserId} has been deactivated", userId);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private (string Token, DateTime ExpiresAt) GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddHours(int.Parse(jwtSettings["ExpiryHours"] ?? "24"));

        var claims = new List<Claim>
        {
            new Claim(AppConstants.Jwt.ClaimUserId, user.Id.ToString()),
            new Claim(AppConstants.Jwt.ClaimEmail, user.Email),
            new Claim(AppConstants.Jwt.ClaimRole, user.Role.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var permissions = GetPermissionsForRole(user.Role);
        foreach (var perm in permissions)
        {
            claims.Add(new Claim(AppConstants.Jwt.ClaimPermission, perm));
        }

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private List<string> GetPermissionsForRole(UserRole role)
    {
        return role switch
        {
            UserRole.Admin => new List<string> { 
                AppConstants.Permissions.CanManageUsers, 
                AppConstants.Permissions.CanViewBilling, 
                AppConstants.Permissions.CanManageInventory,
                AppConstants.Permissions.CanViewSystemSettings,
                AppConstants.Permissions.CanViewReports,
                AppConstants.Permissions.CanManageAppointments 
            },
            UserRole.Doctor => new List<string> { 
                AppConstants.Permissions.CanCreatePrescription, 
                AppConstants.Permissions.CanUploadReports 
            },
            UserRole.Receptionist => new List<string> { 
                AppConstants.Permissions.CanManageAppointments, 
                AppConstants.Permissions.CanAdmitPatients,
                AppConstants.Permissions.CanViewBilling 
            },
            UserRole.LabTechnician => new List<string> { 
                AppConstants.Permissions.CanUploadReports 
            },
            UserRole.Pharmacist => new List<string> { 
                AppConstants.Permissions.CanManageInventory,
                AppConstants.Permissions.CanViewBilling 
            },
            _ => new List<string>() // Patients get no elevated permissions
        };
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
            ValidateLifetime = false // we want to extract claims from expired token
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token algorithm");

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private async Task<string> ResolveFullNameAsync(User user, CancellationToken ct)
    {
        if (user.Role == UserRole.Patient)
        {
            var patient = await _uow.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id, ct);
            return patient != null ? $"{patient.FirstName} {patient.LastName}" : user.Email;
        }
        if (user.Role == UserRole.Doctor)
        {
            var doctor = await _uow.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id, ct);
            return doctor != null ? $"Dr. {doctor.FirstName} {doctor.LastName}" : user.Email;
        }
        return user.Email;
    }
}
