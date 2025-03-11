using System.Net.Http.Json;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using UniTrackRemaster.Services.Messaging.Dto;

namespace UniTrackRemaster.Services.Messaging;

public class ImapService(IConfiguration configuration) : IImapService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly string? _clientId = configuration["Gmail:OAuth2:ClientId"];
    private readonly string? _clientSecret = configuration["Gmail:OAuth2:ClientSecret"];
    private readonly string? _refreshToken = configuration["Gmail:OAuth2:RefreshToken"];
    private readonly string? _emailAddress = configuration["Gmail:EmailAddress"];

    private async Task<string?> GetAccessTokenAsync()
    {
        using var client = new HttpClient();
        const string tokenEndpoint = "https://oauth2.googleapis.com/token";
        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("client_id", _clientId ?? throw new InvalidOperationException()),
            new KeyValuePair<string, string>("client_secret", _clientSecret ?? throw new InvalidOperationException()),
            new KeyValuePair<string, string>("refresh_token", _refreshToken ?? throw new InvalidOperationException()),
            new KeyValuePair<string, string>("grant_type", "refresh_token")
        ]);

        var response = await client.PostAsync(tokenEndpoint, content);
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        if(result is null) throw new InvalidOperationException();
        
        return result.AccessToken;
    }

    private async Task<(IMailFolder Inbox, ImapClient Client)> GetConnectedClientAsync(FolderAccess access = FolderAccess.ReadOnly)
    {
        var client = new ImapClient();
        await client.ConnectAsync("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);

        var oauth2Token = await GetAccessTokenAsync();
    
        if (string.IsNullOrEmpty(_emailAddress))
            throw new InvalidOperationException("Email address is not configured");
        
        if (string.IsNullOrEmpty(oauth2Token))
            throw new InvalidOperationException("Failed to get OAuth2 token");

        var saslMechanism = new SaslMechanismOAuth2(_emailAddress, oauth2Token);
        await client.AuthenticateAsync(saslMechanism);

        var inbox = client.Inbox;
        await inbox.OpenAsync(access);
    
        return (Inbox: inbox, Client: client);
    }

    private async Task<IMailFolder> GetOrCreateFolderAsync(ImapClient client, string folderName)
    {
        try
        {
            // Try to get the folder directly
            var folder = await client.GetFolderAsync(folderName);
            if (folder != null && folder.Exists) return folder;
        }
        catch (FolderNotFoundException)
        {
            // Folder doesn't exist; proceed to create it
        }

        try
        {
            // Gmail uses the first namespace as the root
            var rootFolder = client.GetFolder(client.PersonalNamespaces[0]);

            // Start creating the folder hierarchy
            var pathParts = folderName.Split('/');
            var currentFolder = rootFolder ?? throw new ArgumentNullException(nameof(rootFolder));

            foreach (var part in pathParts)
            {
                // Try to get the subfolder
                var subFolder = await currentFolder.GetSubfolderAsync(part);

                if (subFolder is not { Exists: true })
                {
                    subFolder = await currentFolder.CreateAsync(part, false);
                }

                currentFolder = subFolder;
            }

            return currentFolder;
        }
        catch (Exception ex)
        {
            throw new FolderNotFoundException($"Failed to create or access folder '{folderName}' on Gmail: {ex.Message}");
        }
    }



    public async Task<IList<MimeMessage>> GetRecentEmailsAsync(int count = 10)
    {
        var (inbox, client) = await GetConnectedClientAsync();
        using (client)
        {
            var messages = new List<MimeMessage>();
            for (var i = Math.Max(0, inbox.Count - count); i < inbox.Count; i++)
            {
                var message = await inbox.GetMessageAsync(i);
                messages.Add(await inbox.GetMessageAsync(i));
            }

            await client.DisconnectAsync(true);
            return messages;
        }
    }
    
    public async Task<IList<MimeMessage>> GetUnprocessedEmailsAsync(int count = 10)
    {
        var (inbox, client) = await GetConnectedClientAsync();
        using (client)
        {
            var messages = new List<MimeMessage>();

            try
            {
                // Search for unprocessed emails (not in "Processed" folder)
                var unprocessedQuery = SearchQuery.Not(SearchQuery.HeaderContains("X-Processed", "true"));

                var unprocessedResults = await inbox.SearchAsync(unprocessedQuery);

                // Get the most recent unprocessed emails
                var recentUnprocessedResults = unprocessedResults
                    .Skip(Math.Max(0, unprocessedResults.Count - count))
                    .Take(count);

                foreach (var index in recentUnprocessedResults)
                {
                    var message = await inbox.GetMessageAsync(index);
                    messages.Add(message);
                }

                await client.DisconnectAsync(true);
            }
            catch (Exception)
            {
                await client.DisconnectAsync(true);
                throw;
            }

            return messages;
        }
    }


    public async Task<MimeMessage> GetEmailByIdAsync(string messageId)
    {
        var (inbox, client) = await GetConnectedClientAsync();
        using (client)
        {
            var query = SearchQuery.HeaderContains("Message-ID", messageId);
            var results = await inbox.SearchAsync(query);
            
            if (!results.Any())
                throw new KeyNotFoundException($"Email with Message-ID {messageId} not found.");

            var message = await inbox.GetMessageAsync(results[0]);
            await client.DisconnectAsync(true);
            return message;
        }
    }

    public async Task<IList<MimeMessage>> SearchEmailsAsync(string searchTerm)
    {
        var (inbox, client) = await GetConnectedClientAsync();
        using (client)
        {
            // Chain OR conditions for subject, from, and body
            var query = SearchQuery.Or(
                SearchQuery.SubjectContains(searchTerm),
                SearchQuery.Or(
                    SearchQuery.FromContains(searchTerm),
                    SearchQuery.BodyContains(searchTerm)
                )
            );

            var results = await inbox.SearchAsync(query);
            var messages = new List<MimeMessage>();

            foreach (var uid in results)
            {
                messages.Add(await inbox.GetMessageAsync(uid));
            }

            await client.DisconnectAsync(true);
            return messages;
        }
    }

    public async Task<IList<MimeMessage>> GetEmailsByDateRangeAsync(DateTime start, DateTime end)
    {
        var (inbox, client) = await GetConnectedClientAsync();
        using (client)
        {
            var query = SearchQuery.And(
                SearchQuery.DeliveredAfter(start),
                SearchQuery.DeliveredBefore(end)
            );

            var results = await inbox.SearchAsync(query);
            var messages = new List<MimeMessage>();

            foreach (var uid in results)
            {
                messages.Add(await inbox.GetMessageAsync(uid));
            }

            await client.DisconnectAsync(true);
            return messages;
        }
    }
    
    public async Task MarkEmailAsProcessedAsync(string messageId)
    {
        var (inbox, client) = await GetConnectedClientAsync(FolderAccess.ReadWrite);
        using (client)
        {
            try 
            {
                // First, mark as read
                var query = SearchQuery.HeaderContains("Message-ID", messageId);
                var results = await inbox.SearchAsync(query);
    
                if (!results.Any())
                    throw new KeyNotFoundException($"Email with Message-ID {messageId} not found.");
    
                await inbox.AddFlagsAsync(results[0], MessageFlags.Seen, true);
    
                // Then move to Processed folder/label
                var processedFolder = await GetOrCreateFolderAsync(client, "Processed");
                await inbox.MoveToAsync(results[0], processedFolder);
    
                await client.DisconnectAsync(true);
            }
            catch (Exception)
            {
                await client.DisconnectAsync(true);
                throw;
            }
        }
    }

    
    public async Task MoveEmailToFolderAsync(string messageId, string folderName)
    {
        var (inbox, client) = await GetConnectedClientAsync(FolderAccess.ReadWrite);
        using (client)
        {
            var destinationFolder = await GetOrCreateFolderAsync(client, folderName);

            var query = SearchQuery.HeaderContains("Message-ID", messageId);
            var results = await inbox.SearchAsync(query);

            if (!results.Any())
                throw new KeyNotFoundException($"Email with Message-ID {messageId} not found.");

            await inbox.MoveToAsync(results[0], destinationFolder);
            await client.DisconnectAsync(true);
        }
    }
}
