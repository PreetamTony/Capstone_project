namespace HospitalManagement.DataAccess.Exceptions;

/// <summary>
/// Thrown when a user attempts an action they are not authorized to perform (maps to HTTP 401).
/// </summary>
public class UnauthorizedAccessException : Exception
{
    public UnauthorizedAccessException(string message) : base(message) { }
}
