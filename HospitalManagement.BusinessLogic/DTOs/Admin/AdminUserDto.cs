using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.BusinessLogic.DTOs.Admin;

public class CreateUserRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
}

public class CreateUserResponseDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
}

public class UpdateUserRequestDto
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsActive { get; set; }
}

public class UpdateUserRoleRequestDto
{
    [Required]
    public string Role { get; set; } = string.Empty;
}

public class UpdateUserEmailRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordResponseDto
{
    public string NewPassword { get; set; } = string.Empty;
}

public class UserStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public Dictionary<string, int> UsersByRole { get; set; } = new();
}

public class BulkCreateUsersRequestDto
{
    [Required]
    public List<CreateUserRequestDto> Users { get; set; } = new();
}

public class BulkCreateUsersResponseDto
{
    public int Successful { get; set; }
    public int Failed { get; set; }
    public List<CreateUserResponseDto> CreatedUsers { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
