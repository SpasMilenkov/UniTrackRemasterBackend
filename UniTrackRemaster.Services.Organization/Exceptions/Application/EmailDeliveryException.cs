namespace UniTrackRemaster.Services.Organization.Exceptions.Application;

/// <summary>
/// Thrown when an email operation fails
/// </summary>
public class EmailDeliveryException : ApplicationException
{
    public string Email { get; }
    public string EmailType { get; }
    
    public EmailDeliveryException(string email, string emailType, string message) 
        : base($"Failed to send {emailType} email to '{email}': {message}")
    {
        Email = email;
        EmailType = emailType;
    }
    
    public EmailDeliveryException(string email, string emailType, Exception innerException) 
        : base($"Failed to send {emailType} email to '{email}': {innerException.Message}", innerException)
    {
        Email = email;
        EmailType = emailType;
    }
}