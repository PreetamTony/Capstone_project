namespace HospitalManagement.DataAccess.Exceptions;

/// <summary>
/// Thrown when a concurrent update conflict is detected (maps to HTTP 409).
/// </summary>
public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message) { }
}
