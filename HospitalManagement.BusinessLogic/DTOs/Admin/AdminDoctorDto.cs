using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.BusinessLogic.DTOs.Admin;

public class DoctorSummaryDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public decimal ConsultationFee { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DoctorDetailDto : DoctorSummaryDto
{
    public string Qualification { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public int PendingLeaveRequests { get; set; }
    public decimal TotalRevenueGenerated { get; set; }
}

public class UpdateDoctorRequestDto
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public decimal ConsultationFee { get; set; }
    public Guid? DepartmentId { get; set; }
    public bool IsActive { get; set; }
}

public class DoctorStatsDto
{
    public int TotalDoctors { get; set; }
    public int ActiveDoctors { get; set; }
    public int DoctorsOnLeave { get; set; }
    public Dictionary<string, int> DoctorsByDepartment { get; set; } = new();
    public decimal AverageConsultationFee { get; set; }
    public List<TopDoctorDto> TopDoctorsByPatients { get; set; } = new();
    public List<TopDoctorDto> TopDoctorsByRevenue { get; set; } = new();
}

public class TopDoctorDto
{
    public Guid DoctorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ValueInt { get; set; }
    public decimal ValueDecimal { get; set; }
}
