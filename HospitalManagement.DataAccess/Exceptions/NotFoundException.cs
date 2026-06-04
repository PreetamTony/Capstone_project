namespace HospitalManagement.DataAccess.Exceptions;

/// <summary>
/// Thrown when a requested resource is not found (maps to HTTP 404).
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with identifier '{key}' was not found.") { }
}
