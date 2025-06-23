namespace UniTrackRemaster.Services.Organization.Exceptions.Application;

/// <summary>
/// Thrown when a verification code has expired
/// </summary>
public class ExpiredVerificationCodeException : ApplicationException
{
    public string Email { get; }
    public DateTime ExpirationTime { get; }
    
    public ExpiredVerificationCodeException(string email, DateTime expirationTime) 
        : base($"Verification code for email '{email}' expired on {expirationTime:yyyy-MM-dd HH:mm:ss}.")
    {
        Email = email;
        ExpirationTime = expirationTime;
    }
}
