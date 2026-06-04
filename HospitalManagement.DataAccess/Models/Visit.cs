using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Models.Emr;

namespace HospitalManagement.DataAccess.Models;

public class Visit : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public Guid? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public VisitStatus Status { get; set; } = VisitStatus.CheckedIn;
    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
    public DateTime? DischargeTime { get; set; }
    
    public string? ChiefComplaint { get; set; }
    public string? Diagnosis { get; set; }
    public string? ClinicalNotes { get; set; }

    // Navigation properties for related clinical data
    public ICollection<Vitals> Vitals { get; set; } = new List<Vitals>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<LabReport> LabReports { get; set; } = new List<LabReport>();
    public Billing? Billing { get; set; }
}
