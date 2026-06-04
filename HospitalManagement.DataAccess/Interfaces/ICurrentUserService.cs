namespace HospitalManagement.DataAccess.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    string? Name { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
