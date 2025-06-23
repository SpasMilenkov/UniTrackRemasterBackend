namespace UniTrackRemaster.Services.Organization.Exceptions.Application;

/// <summary>
/// Thrown when attempting to create a duplicate resource
/// </summary>
public class DuplicateApplicationException : ApplicationException
{
    public string Email { get; }
    
    public DuplicateApplicationException(string email) 
        : base($"An application already exists for email '{email}'.")
    {
        Email = email;
    }
}
