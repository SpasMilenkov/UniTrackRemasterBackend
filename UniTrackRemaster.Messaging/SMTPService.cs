using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Net.Http.Json;
using UniTrackRemaster.Messaging.Dto;

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

    public async Task SendEmailAsync(string firstName, string lastName, string emailAddress, string link, string templateType)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("UniTrack", _emailAddress));
        email.To.Add(new MailboxAddress($"{firstName} {lastName}", emailAddress));

        var emailContent = templateType.ToLower() switch
        {
            "verification" => await templateService.GetTemplateAsync("ConfirmEmail"),
            "resetPassword" => await templateService.GetTemplateAsync("ResetPassword"),
            "processed" => await templateService.GetTemplateAsync("ApplicationProcessed"),
            "initial" => await templateService.GetTemplateAsync("InitialResponse"),
            _ => throw new ArgumentException("Invalid template type", nameof(templateType))
        };

        email.Subject = templateType.ToLower() switch
        {
            "verification" => "Confirm Your Email",
            "resetPassword" => "Reset Your Password",
            "processed" => "Application Processed",
            "initial" => "Application Received",
            _ => "UniTrack Notification"
        };

        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = emailContent.Replace("{LINK_TOKEN}", link)
        };

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
}

