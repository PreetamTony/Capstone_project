namespace HospitalManagement.DataAccess.Models;

/// <summary>
/// Medical prescription issued during an appointment.
/// EditableUntil = CreatedAt + 30 minutes (enforced in service layer).
/// </summary>
public class Prescription : BaseEntity
{
    public Guid VisitId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public string? Instructions { get; set; }
    public bool IsDispensed { get; set; } = false;
    public Guid? DispensedBy { get; set; }
    public DateTime? DispensedAt { get; set; }
    public bool IsVoided { get; set; } = false;
    public string? VoidReason { get; set; }

    /// <summary>Computed: CreatedAt + 30 minutes. Enforced in service layer.</summary>
    public DateTime EditableUntil { get; set; }

    // Navigation
    public Visit Visit { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public Patient Patient { get; set; } = null!;

    public bool IsEditable => DateTime.UtcNow <= EditableUntil && !IsVoided;
}
