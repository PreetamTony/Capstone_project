using HospitalManagement.DataAccess.Models.Enums;

namespace HospitalManagement.DataAccess.Models;

/// <summary>
/// Hospital appointment between a patient and a doctor.
/// </summary>
public class Appointment : BaseEntity
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime AppointmentTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public AppointmentType Type { get; set; } = AppointmentType.InPerson;
    public string Reason { get; set; } = string.Empty;

    /// <summary>JSON array of symptom strings.</summary>
    public string? SymptomsJson { get; set; }

    public AppointmentPriority Priority { get; set; } = AppointmentPriority.Medium;
    public string? CancellationReason { get; set; }
    public Guid? CancelledBy { get; set; }
    public bool ReminderSent { get; set; } = false;
    public bool LateCancellationPenalty { get; set; } = false;
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public Visit? Visit { get; set; }
}
