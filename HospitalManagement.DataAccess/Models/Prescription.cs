using HospitalManagement.DataAccess.Models.Enums;

namespace HospitalManagement.DataAccess.Models;

/// <summary>
/// Medical prescription issued during a consultation. Contains multiple items.
/// </summary>
public class Prescription : BaseEntity
{
    public Guid ConsultationId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }

    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Draft;
    
    public DateTime? FinalizedAt { get; set; }
    public DateTime? DispensedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    
    public string? Notes { get; set; }

    public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();

    // Navigation
    public Consultation Consultation { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}
