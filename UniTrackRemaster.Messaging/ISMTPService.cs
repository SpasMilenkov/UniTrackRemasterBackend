
namespace UniTrackRemaster.Messaging;

public interface ISmtpService
{
    public Task SendEmailAsync(string firstName, string lastName, string emailAddress, string link,
        string templateType);
}