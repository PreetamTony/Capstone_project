using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.BusinessLogic.DTOs.Auth;

public class VerifyEmailRequestDto
{
    [Required]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Token { get; set; } = string.Empty;
}

public class ResendVerificationEmailRequestDto
{
    [Required]
    public string Email { get; set; } = string.Empty;
}
