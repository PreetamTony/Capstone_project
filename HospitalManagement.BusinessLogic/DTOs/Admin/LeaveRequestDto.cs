using System;

namespace HospitalManagement.BusinessLogic.DTOs.Admin;

public class LeaveRequestDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Pending, Approved, Rejected, Cancelled
    public DateTime CreatedAt { get; set; }
}
