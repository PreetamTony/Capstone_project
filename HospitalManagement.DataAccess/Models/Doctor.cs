namespace HospitalManagement.DataAccess.Models;

/// <summary>
/// Doctor profile linked to a User identity.
/// </summary>
public class Doctor : BaseEntity
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public List<string> Languages { get; set; } = new List<string>();
    public string LicenseNumber { get; set; } = string.Empty;
    
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public int YearsOfExperience { get; set; }
    public decimal ConsultationFee { get; set; }
    public int MaxPatientsPerDay { get; set; } = 20;
    public int AverageConsultationMinutes { get; set; } = 30;
    
    public decimal Rating { get; set; } = 0;

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public string FullName => $"Dr. {FirstName} {LastName}";
}
