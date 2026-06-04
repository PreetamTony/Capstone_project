namespace HospitalManagement.BusinessLogic.DTOs.Prescription;

public class PrescriptionResponseDto
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public string? Instructions { get; set; }
    public bool IsDispensed { get; set; }
    public DateTime? DispensedAt { get; set; }
    public bool IsVoided { get; set; }
    public string? VoidReason { get; set; }
    public bool IsEditable { get; set; }
    public DateTime EditableUntil { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdatePrescriptionRequestDto
{
    public string? MedicationName { get; set; }
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public int? DurationDays { get; set; }
    public string? Instructions { get; set; }
}

public class VoidPrescriptionRequestDto
{
    public string Reason { get; set; } = string.Empty;
}
