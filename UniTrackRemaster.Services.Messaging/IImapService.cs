using MailKit;
using MailKit.Net.Imap;
using MimeKit;

namespace UniTrackRemaster.Services.Messaging;


public interface IImapService
{
    Task<IList<MimeMessage>> GetRecentEmailsAsync(int count = 10);
    Task<IList<MimeMessage>> GetUnprocessedEmailsAsync(int count = 10);
    Task<MimeMessage> GetEmailByIdAsync(string messageId);
    Task<IList<MimeMessage>> SearchEmailsAsync(string searchTerm);
    Task<IList<MimeMessage>> GetEmailsByDateRangeAsync(DateTime start, DateTime end);
    Task MarkEmailAsProcessedAsync(string messageId);
    Task MoveEmailToFolderAsync(string messageId, string folderName);
}