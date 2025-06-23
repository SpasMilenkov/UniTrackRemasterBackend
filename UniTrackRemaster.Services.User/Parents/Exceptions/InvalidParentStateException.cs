namespace UniTrackRemaster.Services.User.Parents.Exceptions;

/// <summary>
/// Exception thrown when attempting to perform an operation on a parent in an invalid state
/// </summary>
public class InvalidParentStateException : ParentException
{
    public Guid ParentId { get; }
    public string CurrentState { get; }
    public string RequiredState { get; }

    public InvalidParentStateException(Guid parentId, string currentState, string requiredState) 
        : base($"Parent {parentId} is in state '{currentState}' but operation requires '{requiredState}'")
    {
        ParentId = parentId;
        CurrentState = currentState;
        RequiredState = requiredState;
    }
}