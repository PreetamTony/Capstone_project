using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.BusinessLogic.DTOs.Visit;

public class CreateConsultationRequestDto
{
    [Required]
    public Guid VisitId { get; set; }

    [Required]
    public string Symptoms { get; set; } = string.Empty;

    [Required]
    public string Diagnosis { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string Recommendations { get; set; } = string.Empty;

    public string Status { get; set; } = "Draft"; // Draft or Completed
}

public class ConsultationResponseDto
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public string Symptoms { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
