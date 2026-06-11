using System;

namespace HospitalManagement.BusinessLogic.DTOs.Department;

public class DepartmentStatisticsDto
{
    public int TotalDoctors { get; set; }
    public int TotalAppointments { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class DepartmentDoctorDto
{
    public Guid DoctorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Qualifications { get; set; } = string.Empty;
    public bool IsHead { get; set; }
}
