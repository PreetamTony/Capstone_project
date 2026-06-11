namespace HospitalManagement.DataAccess.Models;

/// <summary>
/// Represents a clinical or administrative department in the hospital.
/// </summary>
public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? HeadDoctorId { get; set; }

    // Navigation
    public Doctor? HeadDoctor { get; set; }
    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
