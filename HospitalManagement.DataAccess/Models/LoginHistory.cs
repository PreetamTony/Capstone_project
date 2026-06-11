using System;

namespace HospitalManagement.DataAccess.Models;

public class LoginHistory : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime Timestamp { get; set; }
    
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    public bool IsSuccess { get; set; }
    public string? FailureReason { get; set; }
}
