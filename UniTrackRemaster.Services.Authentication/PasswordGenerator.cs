using System.Text;

namespace UniTrackRemaster.Services.Authentication;

//TODO: Replace with cryptographic algorithm if needed
public class PasswordGenerator : IPasswordGenerator
{
    private readonly Random _random;
    
    public PasswordGenerator()
    {
        _random = new Random(Guid.NewGuid().GetHashCode());
    }
    
    public string GenerateSecurePassword()
    {
        const string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        const string numberChars = "0123456789";
        const string specialChars = "!@#$%^&*";
        
        var password = new StringBuilder();
        
        // Ensure at least one of each type
        password.Append(uppercaseChars[_random.Next(uppercaseChars.Length)]);
        password.Append(lowercaseChars[_random.Next(lowercaseChars.Length)]);
        password.Append(numberChars[_random.Next(numberChars.Length)]);
        password.Append(specialChars[_random.Next(specialChars.Length)]);
        
        // Fill the rest
        const string allChars = uppercaseChars + lowercaseChars + numberChars + specialChars;
        for (int i = 4; i < 16; i++)
        {
            password.Append(allChars[_random.Next(allChars.Length)]);
        }
        
        // Shuffle the password
        return new string(password.ToString().ToCharArray().OrderBy(x => _random.Next()).ToArray());
    }
}