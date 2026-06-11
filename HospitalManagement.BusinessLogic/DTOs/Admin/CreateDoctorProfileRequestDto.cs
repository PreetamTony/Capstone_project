namespace HospitalManagement.BusinessLogic.DTOs.Admin;

public class CreateDoctorProfileRequestDto
{
    public Guid UserId { get; set; }
    public Guid DepartmentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public decimal ConsultationFee { get; set; }
    public string? LicenseNumber { get; set; }
}
