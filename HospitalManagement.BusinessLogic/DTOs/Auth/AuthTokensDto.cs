namespace HospitalManagement.BusinessLogic.DTOs.Auth;

public class RefreshTokenRequestDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class RevokeTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}
