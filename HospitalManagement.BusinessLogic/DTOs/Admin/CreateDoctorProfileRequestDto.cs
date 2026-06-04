using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.BusinessLogic.DTOs.Admin;

public class CreateDoctorProfileRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid DepartmentId { get; set; }

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;
    
    public string Qualification { get; set; } = string.Empty;

    [Range(0, 100)]
    public int ExperienceYears { get; set; }
    
    public decimal ConsultationFee { get; set; }
}
