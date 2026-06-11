using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Models.Emr;

namespace HospitalManagement.DataAccess.Models;

public class Visit : BaseEntity
{
    public string VisitNumber { get; set; } = string.Empty;

    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public Guid? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public VisitType VisitType { get; set; } = VisitType.OPD;
    public VisitStatus Status { get; set; } = VisitStatus.CheckedIn;
    
    public string? ChiefComplaint { get; set; }
    public string? Notes { get; set; }

    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
    public DateTime? DischargeTime { get; set; }

    // Auditing
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    
    public DateTime? CancelledAt { get; set; }
    public Guid? CancelledBy { get; set; }
    public string? CancellationReason { get; set; }

    public string? QueueNumber { get; set; }
    public string? RoomNumber { get; set; }

    public Consultation? Consultation { get; set; }


    // Navigation properties for related clinical data
    public ICollection<Vitals> Vitals { get; set; } = new List<Vitals>();
}
