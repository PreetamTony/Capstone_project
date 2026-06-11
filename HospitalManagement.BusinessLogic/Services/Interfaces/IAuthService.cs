using HospitalManagement.BusinessLogic.DTOs.Auth;
using HospitalManagement.BusinessLogic.DTOs.Common;

namespace HospitalManagement.BusinessLogic.Services;

/// <summary>Authentication and user management service contract.</summary>
public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default);
    Task<Guid> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken ct = default);
    Task ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken ct = default);
    Task ResetPasswordWithTokenAsync(ResetPasswordWithTokenRequestDto request, CancellationToken ct = default);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken ct = default);
    Task RevokeTokenAsync(RevokeTokenRequestDto request, CancellationToken ct = default);
    Task<CurrentUserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);
    Task VerifyEmailAsync(VerifyEmailRequestDto request, CancellationToken ct = default);
    Task ResendVerificationEmailAsync(ResendVerificationEmailRequestDto request, CancellationToken ct = default);
    Task<PagedResult<LoginHistoryDto>> GetMyLoginHistoryAsync(Guid userId, int pageNumber = 1, int pageSize = 20, CancellationToken ct = default);
    Task<PagedResult<LoginHistoryDto>> GetUserLoginHistoryAsync(Guid userId, int pageNumber = 1, int pageSize = 20, CancellationToken ct = default);
}
