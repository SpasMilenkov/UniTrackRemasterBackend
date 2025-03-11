using UniTrackRemaster.Services.Messaging.Enums;

namespace UniTrackRemaster.Services.Messaging;

public interface ISmtpService
{
    public Task SendEmailAsync(string firstName, string lastName, string emailAddress, string? link,
        EmailTemplateType templateType);

    public Task SendEmailAsync(string emailAddress, string? link, EmailTemplateType templateType);
    public Task SendEmailWithCodeAsync(string firstName, string lastName, string emailAddress, string code,
        EmailTemplateType templateType);

    Task SendAdminCredentialsEmailAsync(string emailAddress, string username, string password);
}