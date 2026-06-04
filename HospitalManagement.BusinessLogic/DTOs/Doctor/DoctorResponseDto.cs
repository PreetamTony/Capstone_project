namespace HospitalManagement.BusinessLogic.DTOs.Doctor;

public class DoctorResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public decimal ConsultationFee { get; set; }
    public int MaxPatientsPerDay { get; set; }
    public int AverageConsultationMinutes { get; set; }
    public bool IsAvailable { get; set; }
    public decimal Rating { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
