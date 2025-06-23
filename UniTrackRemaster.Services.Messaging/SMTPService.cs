using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Services.Messaging.Enums;

namespace UniTrackRemaster.Services.Messaging;

public class SmtpService(IConfiguration configuration, ITemplateService templateService) : ISmtpService
{
    private readonly string? _smtpHost = configuration["Mailtrap:Host"];
    private readonly int _smtpPort = configuration.GetValue("Mailtrap:Port", 587);
    private readonly string? _username = configuration["Mailtrap:Username"];
    private readonly string? _password = configuration["Mailtrap:Password"];
    private readonly string? _fromEmail = configuration["Mailtrap:FromEmail"];
    private readonly string? _fromName = configuration["Mailtrap:FromName"];

    public async Task SendEmailAsync(string firstName, string lastName, string emailAddress, string? link, EmailTemplateType templateType)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_fromName ?? "UniTrack", _fromEmail));
        email.To.Add(new MailboxAddress($"{firstName} {lastName}", emailAddress));

        var emailContent = templateType switch
        {
            EmailTemplateType.Verification => await templateService.GetTemplateAsync("ConfirmEmail"),
            EmailTemplateType.ResetPassword => await templateService.GetTemplateAsync("ResetPassword"),
            EmailTemplateType.Processed => await templateService.GetTemplateAsync("ApplicationProcessed"),
            EmailTemplateType.Initial => await templateService.GetTemplateAsync("InitialResponse"),
            EmailTemplateType.ApplicationDenied => await templateService.GetTemplateAsync("ApplicationDenied"),
            EmailTemplateType.AdminCredentials => await templateService.GetTemplateAsync("AdminCredentials"),
            _ => throw new ArgumentException("Invalid template type", nameof(templateType))
        };

        email.Subject = templateType switch
        {
            EmailTemplateType.Verification => "Confirm Your Email",
            EmailTemplateType.ResetPassword => "Reset Your Password",
            EmailTemplateType.Processed => "Application Processed",
            EmailTemplateType.Initial => "Application Received",
            EmailTemplateType.AdminCredentials => "Your UniTrack Admin Account",
            _ => throw new ArgumentException("Invalid template type", nameof(templateType))
        };

        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = emailContent.Replace("{LINK_TOKEN}", link)
        };

        await SendEmailInternalAsync(email);
    }

    public async Task SendEmailAsync(string emailAddress, string? link, EmailTemplateType templateType)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_fromName ?? "UniTrack", _fromEmail));
        email.To.Add(new MailboxAddress(emailAddress, emailAddress));

        var emailContent = templateType switch
        {
            EmailTemplateType.Verification => await templateService.GetTemplateAsync("ConfirmEmail"),
            EmailTemplateType.ResetPassword => await templateService.GetTemplateAsync("ResetPassword"),
            EmailTemplateType.Processed => await templateService.GetTemplateAsync("ApplicationProcessed"),
            EmailTemplateType.Initial => await templateService.GetTemplateAsync("InitialResponse"),
            EmailTemplateType.ApplicationDenied => await templateService.GetTemplateAsync("ApplicationDenied"),
            EmailTemplateType.AdminCredentials => await templateService.GetTemplateAsync("AdminCredentials"),
            _ => throw new ArgumentException("Invalid template type", nameof(templateType))
        };

        email.Subject = templateType switch
        {
            EmailTemplateType.Verification => "Confirm Your Email",
            EmailTemplateType.ResetPassword => "Reset Your Password",
            EmailTemplateType.Processed => "Application Processed",
            EmailTemplateType.Initial => "Application Received",
            EmailTemplateType.AdminCredentials => "Your UniTrack Admin Account",
            _ => throw new ArgumentException("Invalid template type", nameof(templateType))
        };

        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = emailContent.Replace("{LINK_TOKEN}", link)
        };

        await SendEmailInternalAsync(email);
    }

    public async Task SendEmailWithCodeAsync(string firstName, string lastName, string emailAddress, string code, EmailTemplateType templateType)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_fromName ?? "UniTrack", _fromEmail));
        email.To.Add(new MailboxAddress($"{firstName} {lastName}", emailAddress));

        var emailContent = templateType switch
        {
            EmailTemplateType.ApplicationApproved => await templateService.GetTemplateAsync("ApplicationApproved"),
            EmailTemplateType.ApplicationCreated => await templateService.GetTemplateAsync("ApplicationCreated"),
            _ => throw new ArgumentException("Invalid template type for code-based email", nameof(templateType))
        };

        emailContent = emailContent.Replace("{CODE}", code);

        email.Subject = templateType switch
        {
            EmailTemplateType.ApplicationApproved => "Application Approved - Your Access Code",
            EmailTemplateType.ApplicationCreated => "Application Created - Your Access Code",
            _ => throw new ArgumentException("Invalid template type", nameof(templateType))
        };

        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = emailContent
        };

        await SendEmailInternalAsync(email);
    }

    public async Task SendAdminCredentialsEmailAsync(string emailAddress, string username, string password)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_fromName ?? "UniTrack", _fromEmail));
        email.To.Add(new MailboxAddress(emailAddress, emailAddress));

        var emailContent = await templateService.GetTemplateAsync("AdminCredentials");

        // Replace the credential placeholders
        emailContent = emailContent
            .Replace("{Username}", username)
            .Replace("{Password}", password);

        email.Subject = "Your UniTrack Admin Account";

        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = emailContent
        };

        await SendEmailInternalAsync(email);
    }

    // Private helper to avoid code duplication
    private async Task SendEmailInternalAsync(MimeMessage email)
    {
        using var smtp = new SmtpClient();

        try
        {
            // Connect to Mailtrap SMTP server
            await smtp.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);

            // Authenticate with username and password (much simpler than OAuth2)
            await smtp.AuthenticateAsync(_username, _password);

            await smtp.SendAsync(email);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
        }
        finally
        {
            await smtp.DisconnectAsync(true);
        }
    }
}