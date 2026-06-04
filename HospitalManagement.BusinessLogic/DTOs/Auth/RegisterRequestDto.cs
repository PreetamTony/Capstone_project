using HospitalManagement.DataAccess.Models.Enums;

namespace HospitalManagement.BusinessLogic.DTOs.Auth;

public class RegisterRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? Address { get; set; }
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public decimal InsuranceCoveragePercent { get; set; } = 0;
}
