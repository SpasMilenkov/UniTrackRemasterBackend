using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using MimeKit;
using NCrontab;
using UniTrackRemaster.Services.Messaging.Dto;
using UniTrackRemaster.Services.Messaging.Enums;

namespace UniTrackRemaster.Services.Messaging;

public class EmailProcessingService(
    IServiceScopeFactory scopeFactory,
    ILogger<EmailProcessingService> logger)
    : IHostedService, IDisposable
{
    private readonly CrontabSchedule _schedule = CrontabSchedule.Parse("*/1 * * * *");
    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Email Processing Service is starting.");

        // Calculate time to next run
        var next = _schedule.GetNextOccurrence(DateTime.Now);
        var delay = next - DateTime.Now;

        _timer = new Timer(ProcessEmails, null, delay, TimeSpan.FromMinutes(1));

        return Task.CompletedTask;
    }

    private async void ProcessEmails(object? state)
    {
        try
        {
            logger.LogInformation("Starting scheduled email processing task at: {time}", DateTime.Now);
            
            using var scope = scopeFactory.CreateScope();
            var imapService = scope.ServiceProvider.GetRequiredService<IImapService>();

            logger.LogInformation("Fetching unread emails from inbox");
            var emails = await imapService.GetUnprocessedEmailsAsync(1);
        
            logger.LogInformation("Found {count} emails in total", emails.Count);
            foreach (var email in emails)
            {
                try 
                {
                    await ProcessSingleEmail(email);
                    await imapService.MarkEmailAsProcessedAsync(email.MessageId);
                    await SendFollowUpEmailAsync(email);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to process email with subject: {subject}", email.Subject);
                }
            }
        
            logger.LogInformation("Completed email processing task at: {time}", DateTime.Now);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process emails batch");
        }
        
    }

    private Task ProcessSingleEmail(MimeMessage email)
    {
        // Extract email data
        var emailData = new EmailData
        {
            Subject = email.Subject,
            From = email.From.ToString(),
            Date = email.Date.DateTime,
            Body = GetEmailBody(email),
            MessageId = email.MessageId
        };

        // Log the processed data - this will be replaced in the future
        LogEmailData(emailData);

        return Task.CompletedTask;
    }

    private string GetEmailBody(MimeMessage email)
    {
        if (email.TextBody != null)
            return email.TextBody;

        if (email.HtmlBody != null)
        {
            // Simple HTML to text conversion - you might want to use a proper HTML parser
            return email.HtmlBody.Replace("<br>", "\n")
                               .Replace("<br/>", "\n")
                               .Replace("<p>", "\n")
                               .Replace("</p>", "\n");
        }

        return "No readable content";
    }

    private void LogEmailData(EmailData emailData)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Processed Email ===");
        sb.AppendLine($"From: {emailData.From}");
        sb.AppendLine($"Subject: {emailData.Subject}");
        sb.AppendLine($"Date: {emailData.Date}");
        sb.AppendLine($"Message-ID: {emailData.MessageId}");
        sb.AppendLine("Body:");
        sb.AppendLine(emailData.Body);
        sb.AppendLine("===================");

        logger.LogInformation(sb.ToString());
    }
    
    private async Task SendFollowUpEmailAsync(MimeMessage email)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var smtpService = scope.ServiceProvider.GetRequiredService<ISmtpService>();

            var recipient = email.From.Mailboxes.FirstOrDefault();
            if (recipient == null) return;

            
            await smtpService.SendEmailAsync(
                firstName: recipient.Name.Split(' ').FirstOrDefault() ?? "User",
                lastName: recipient.Name.Split(' ').LastOrDefault() ?? "",
                emailAddress: recipient.Address,
                link: string.Empty,
                templateType: EmailTemplateType.Processed);

            logger.LogInformation("Sent follow-up email to {address} for processed message {messageId}", recipient.Address, email.MessageId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send follow-up email for message {messageId}", email.MessageId);
        }
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Email Processing Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }
    
    public void Dispose()
    {
        _timer?.Dispose();
    }
}