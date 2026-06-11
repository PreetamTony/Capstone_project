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
    public AppointmentSource Source { get; set; } = AppointmentSource.PatientPortal;

    // Audit Fields
    public Guid? CreatedByUserId { get; set; }
    public string? BookedByRole { get; set; }
    
    // Cancellation
    public string? CancellationReason { get; set; }
    public Guid? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public bool LateCancellationPenalty { get; set; } = false;

    // Rescheduling
    public Guid? RescheduledByUserId { get; set; }
    public DateTime? RescheduledAt { get; set; }

    // Check-in & Completion
    public Guid? CheckInByUserId { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Details
    public string? Notes { get; set; }
    public int? QueueNumber { get; set; }
    public string? ConsultationRoom { get; set; }

    // Telemedicine
    public bool IsTeleConsultation { get; set; } = false;
    public string? MeetingUrl { get; set; }
    public string? MeetingProvider { get; set; }

    // Notifications
    public bool ReminderSent { get; set; } = false;
    public DateTime? ReminderSentAt { get; set; }
    public string? ReminderStatus { get; set; }
    public DateTime? ConfirmationSentAt { get; set; }
    public DateTime? LastNotificationSentAt { get; set; }

    // Navigation
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public Visit? Visit { get; set; }
}
