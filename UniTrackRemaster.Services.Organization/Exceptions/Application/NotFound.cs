namespace UniTrackRemaster.Services.Organization.Exceptions.Application;

/// <summary>
/// Thrown when a requested resource is not found
/// </summary>
public class NotFoundException : ApplicationException
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string resourceType, string identifier) 
        : base($"{resourceType} with identifier '{identifier}' was not found.") { }
}