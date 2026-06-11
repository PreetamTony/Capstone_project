using System;

namespace HospitalManagement.BusinessLogic.DTOs.Auth;

public class LoginHistoryDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsSuccess { get; set; }
    public string? FailureReason { get; set; }
}
