using HospitalManagement.BusinessLogic.DTOs.Auth;

namespace HospitalManagement.BusinessLogic.Services;

/// <summary>Authentication and user management service contract.</summary>
public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default);
    Task<Guid> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken ct = default);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken ct = default);
    Task RevokeTokenAsync(RevokeTokenRequestDto request, CancellationToken ct = default);
    Task DeactivateUserAsync(Guid userId, CancellationToken ct = default);
}
