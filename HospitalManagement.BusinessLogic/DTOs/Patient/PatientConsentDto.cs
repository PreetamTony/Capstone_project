using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.BusinessLogic.DTOs.Patient;

public class UpdatePatientConsentRequestDto
{
    [Required]
    public string ConsentType { get; set; } = string.Empty;

    public bool IsGranted { get; set; }
}

public class PatientConsentResponseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string ConsentType { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
    public DateTime GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
