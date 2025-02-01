using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Net.Http.Json;
using UniTrackRemaster.Messaging.Dto;
using UniTrackRemaster.Messaging.Enums;

namespace UniTrackRemaster.Messaging;

public class SmtpService(IConfiguration configuration, ITemplateService templateService) : ISmtpService
{
    private readonly string? _clientId = configuration["Gmail:OAuth2:ClientId"];
    private readonly string? _clientSecret = configuration["Gmail:OAuth2:ClientSecret"];
    private readonly string? _refreshToken = configuration["Gmail:OAuth2:RefreshToken"];
    private readonly string? _emailAddress = configuration["Gmail:EmailAddress"];

    private async Task<string?> GetAccessTokenAsync()
    {
        using var client = new HttpClient();
        const string tokenEndpoint = "https://oauth2.googleapis.com/token";
        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string?>("client_id", _clientId),
            new KeyValuePair<string, string?>("client_secret", _clientSecret),
            new KeyValuePair<string, string?>("refresh_token", _refreshToken),
            new KeyValuePair<string, string?>("grant_type", "refresh_token")
        ]);

        var response = await client.PostAsync(tokenEndpoint, content);
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return result?.AccessToken;
    }

    public async Task SendEmailAsync(string firstName, string lastName, string emailAddress, string? link, EmailTemplateType templateType)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("UniTrack", _emailAddress));
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
        email.From.Add(new MailboxAddress("UniTrack", _emailAddress));
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

    // Private helper to avoid code duplication
    private async Task SendEmailInternalAsync(MimeMessage email)
    {
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

        // Get OAuth2 access token
        var oauth2Token = await GetAccessTokenAsync();

        // Use SaslMechanismOAuth2 for authentication
        var saslMechanism = new SaslMechanismOAuth2(_emailAddress, oauth2Token);
        await smtp.AuthenticateAsync(saslMechanism);

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
    
    public async Task SendEmailWithCodeAsync(string firstName, string lastName, string emailAddress, string code, EmailTemplateType templateType)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("UniTrack", _emailAddress));
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

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

        var oauth2Token = await GetAccessTokenAsync();
        var saslMechanism = new SaslMechanismOAuth2(_emailAddress, oauth2Token);
        await smtp.AuthenticateAsync(saslMechanism);

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
    // New method specifically for admin credentials
    public async Task SendAdminCredentialsEmailAsync(string emailAddress, string username, string password)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("UniTrack", _emailAddress));
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
}

