namespace UniTrackRemaster.Services.Organization.Exceptions.Application;

/// <summary>
/// Thrown when an operation cannot be performed due to the current state of the application
/// </summary>
public class InvalidApplicationStateException : ApplicationException
{
    public Guid ApplicationId { get; }
    public string CurrentState { get; }
    public string RequiredState { get; }
    
    public InvalidApplicationStateException(Guid applicationId, string currentState, string requiredState) 
        : base($"Application {applicationId} is in state '{currentState}' but operation requires state '{requiredState}'.")
    {
        ApplicationId = applicationId;
        CurrentState = currentState;
        RequiredState = requiredState;
    }
}