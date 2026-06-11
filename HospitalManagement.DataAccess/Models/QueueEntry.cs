using HospitalManagement.DataAccess.Models.Enums;

namespace HospitalManagement.DataAccess.Models;

public class QueueEntry : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public Guid VisitId { get; set; }
    public Visit Visit { get; set; } = null!;

    public int TokenNumber { get; set; }
    
    public QueueStatus Status { get; set; }

    public DateTime CheckedInAt { get; set; }
    public DateTime? CalledAt { get; set; }
    public DateTime? ConsultationStartedAt { get; set; }
    public DateTime? ConsultationEndedAt { get; set; }

    public Guid? CalledBy { get; set; }
    
    public int CallCount { get; set; } = 0;
}
