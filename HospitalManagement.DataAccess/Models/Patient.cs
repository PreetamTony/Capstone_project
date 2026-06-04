using HospitalManagement.DataAccess.Models.Enums;

namespace HospitalManagement.DataAccess.Models;

/// <summary>
/// Patient profile linked to a User identity.
/// </summary>
public class Patient : BaseEntity
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public BloodGroup? BloodGroup { get; set; }
    public string? Address { get; set; }
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public decimal InsuranceCoveragePercent { get; set; } = 0;
    public int NoShowCount { get; set; } = 0;
    public bool IsPriority { get; set; } = false;

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<LabReport> LabReports { get; set; } = new List<LabReport>();
    public ICollection<Billing> Bills { get; set; } = new List<Billing>();
    public ICollection<PatientConsent> Consents { get; set; } = new List<PatientConsent>();
    public Emr.EmrRecord? EmrRecord { get; set; } // Added EMR Record

    // Computed property (not stored in DB)
    public int Age => DateTime.UtcNow.Year - DateOfBirth.Year
        - (DateTime.UtcNow.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);

    public string FullName => $"{FirstName} {LastName}";
}
