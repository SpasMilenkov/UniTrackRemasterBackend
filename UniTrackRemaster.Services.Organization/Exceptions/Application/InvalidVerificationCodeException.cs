namespace UniTrackRemaster.Services.Organization.Exceptions.Application;

/// <summary>
/// Thrown when a verification code is invalid
/// </summary>
public class InvalidVerificationCodeException : ApplicationException
{
    public string ProvidedCode { get; }
    public string Email { get; }
    
    public InvalidVerificationCodeException(string email, string providedCode) 
        : base($"Invalid verification code provided for email '{email}'.")
    {
        Email = email;
        ProvidedCode = providedCode;
    }
}
