namespace UniTrackRemaster.Services.User.Parents.Exceptions;

/// <summary>
/// Base exception for parent-related operations
/// </summary>
public abstract class ParentException : Exception
{
    protected ParentException(string message) : base(message) { }
    protected ParentException(string message, Exception innerException) : base(message, innerException) { }
}
