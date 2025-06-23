namespace UniTrackRemaster.Services.Organization.Exceptions.Application;

/// <summary>
/// Thrown when a business rule is violated
/// </summary>
public class BusinessRuleViolationException : ApplicationException
{
    public string RuleName { get; }
    
    public BusinessRuleViolationException(string ruleName, string message) : base(message)
    {
        RuleName = ruleName;
    }
}