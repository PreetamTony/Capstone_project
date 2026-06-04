namespace HospitalManagement.DataAccess.Models;

public class DoctorReview : BaseEntity
{
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public int Rating { get; set; } // 1 to 5
    public string Comment { get; set; } = string.Empty;
}
