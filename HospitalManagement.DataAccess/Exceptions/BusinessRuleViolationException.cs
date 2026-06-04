namespace HospitalManagement.DataAccess.Exceptions;

/// <summary>
/// Thrown when a business rule is violated (maps to HTTP 400).
/// </summary>
public class BusinessRuleViolationException : Exception
{
    public string Rule { get; }

    public BusinessRuleViolationException(string rule, string message)
        : base(message)
    {
        Rule = rule;
    }
}
