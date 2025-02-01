namespace UniTrackRemaster.Services.Authentication;

public interface IPasswordGenerator
{
    string GenerateSecurePassword();
}