namespace UniTrackRemaster.Services.Organization.Exceptions.Application;

/// <summary>
/// Base exception for all application-related errors
/// </summary>
public abstract class ApplicationException : Exception
{
    protected ApplicationException(string message) : base(message) { }
    protected ApplicationException(string message, Exception innerException) : base(message, innerException) { }
}