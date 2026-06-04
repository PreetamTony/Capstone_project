namespace HospitalManagement.DataAccess.Models;

/// <summary>
/// Tracks patient consents for data sharing, teleconsultation, etc.
/// </summary>
public class PatientConsent : BaseEntity
{
    public Guid PatientId { get; set; }
    public string ConsentType { get; set; } = string.Empty; // e.g., "DataSharing", "Teleconsultation", "EmailNotifications"
    public bool IsGranted { get; set; }
    public DateTime GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    // Navigation
    public Patient Patient { get; set; } = null!;
}
