using System.ComponentModel.DataAnnotations;
using HospitalManagement.BusinessLogic.DTOs.Admin;

namespace HospitalManagement.BusinessLogic.DTOs.Visit;

public class CreateConsultationRequestDto
{
    [Required]
    public Guid VisitId { get; set; }

    [Required]
    public string ChiefComplaint { get; set; } = string.Empty;

    public List<string> Symptoms { get; set; } = new();

    public string Assessment { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string DiagnosisCode { get; set; } = string.Empty;
    public string TreatmentPlan { get; set; } = string.Empty;
    
    public string Notes { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public string FollowUpInstructions { get; set; } = string.Empty;
    public DateTime? FollowUpDate { get; set; }
}

public class UpdateConsultationRequestDto
{
    [Required]
    public string ChiefComplaint { get; set; } = string.Empty;

    public List<string> Symptoms { get; set; } = new();

    public string Assessment { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string DiagnosisCode { get; set; } = string.Empty;
    public string TreatmentPlan { get; set; } = string.Empty;
    
    public string Notes { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public string FollowUpInstructions { get; set; } = string.Empty;
    public DateTime? FollowUpDate { get; set; }
}

public class ConsultationFilterDto
{
    public Guid? DoctorId { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? VisitId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ConsultationSummaryDto
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string ChiefComplaint { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class ConsultationDetailsDto
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }

    public ConsultationDoctorDto? Doctor { get; set; }
    
    public string ChiefComplaint { get; set; } = string.Empty;
    public List<string> Symptoms { get; set; } = new();
    public string Assessment { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string DiagnosisCode { get; set; } = string.Empty;
    public string TreatmentPlan { get; set; } = string.Empty;
    
    public string Notes { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public string FollowUpInstructions { get; set; } = string.Empty;
    public DateTime? FollowUpDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public int PrescriptionCount { get; set; }
    public int LabOrderCount { get; set; }
}

public class ConsultationDoctorDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
