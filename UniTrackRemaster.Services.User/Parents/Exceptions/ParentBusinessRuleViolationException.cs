namespace UniTrackRemaster.Services.User.Parents.Exceptions;

/// <summary>
/// Exception thrown when attempting to perform an operation that violates business rules
/// </summary>
public class ParentBusinessRuleViolationException : ParentException
{
    public string RuleName { get; }

    public ParentBusinessRuleViolationException(string ruleName, string message) 
        : base($"Business rule violation ({ruleName}): {message}")
    {
        RuleName = ruleName;
    }
}