namespace HospitalManagement.BusinessLogic.DTOs.Appointment;

public class AppointmentResponseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorSpecialization { get; set; } = string.Empty;
    public DateTime AppointmentTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public List<string>? Symptoms { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string? CancellationReason { get; set; }
    public bool ReminderSent { get; set; }
    public bool LateCancellationPenalty { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
