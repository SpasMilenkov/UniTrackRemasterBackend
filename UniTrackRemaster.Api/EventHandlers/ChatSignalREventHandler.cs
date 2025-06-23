using Microsoft.AspNetCore.SignalR;
using UniTrackRemaster.Api.Dto.Chat.Events;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Hubs;
namespace UniTrackRemaster.EventHandlers;

public class ChatSignalREventHandler : IHostedService
{
    private readonly IEventBus _eventBus;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ChatSignalREventHandler> _logger;

    public ChatSignalREventHandler(
        IEventBus eventBus,
        IHubContext<ChatHub> hubContext,
        IServiceProvider serviceProvider,
        ILogger<ChatSignalREventHandler> logger)
    {
        _eventBus = eventBus;
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Subscribe to all chat events
        _eventBus.Subscribe<ChatMessageSentEvent>(HandleMessageSentEvent);
        _eventBus.Subscribe<ChatMessageEditedEvent>(HandleMessageEditedEvent);
        _eventBus.Subscribe<ChatMessageDeletedEvent>(HandleMessageDeletedEvent);
        _eventBus.Subscribe<ChatMessageReadEvent>(HandleMessageReadEvent);
        _eventBus.Subscribe<MessageReactionAddedEvent>(HandleReactionAddedEvent);
        _eventBus.Subscribe<MessageReactionRemovedEvent>(HandleReactionRemovedEvent);
        _eventBus.Subscribe<UserTypingEvent>(HandleUserTypingEvent);
        _eventBus.Subscribe<UserStoppedTypingEvent>(HandleUserStoppedTypingEvent);
        _eventBus.Subscribe<UserConnectedEvent>(HandleUserConnectedEvent);
        _eventBus.Subscribe<UserDisconnectedEvent>(HandleUserDisconnectedEvent);

        _logger.LogInformation("ChatSignalREventHandler started and subscribed to events");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ChatSignalREventHandler stopped");
        return Task.CompletedTask;
    }

    // ===== MESSAGE EVENTS =====

    private async Task HandleMessageSentEvent(ChatMessageSentEvent eventData)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var chatService = scope.ServiceProvider.GetRequiredService<IChatService>();
            
            // Get full message details
            var message = await chatService.GetMessageByIdAsync(eventData.MessageId);
            if (message == null) return;

            var messageDto = chatService.MapToDto(message, eventData.SenderId);

            if (eventData.RecipientId.HasValue)
            {
                // Direct message
                await _hubContext.Clients.User(eventData.RecipientId.ToString())
                    .SendAsync("ReceiveDirectMessage", messageDto);
            }
            else if (eventData.GroupId.HasValue)
            {
                // Group message - send to all group members except sender
                await _hubContext.Clients.GroupExcept($"conversation_group_{eventData.GroupId}", 
                    eventData.SenderId.ToString())
                    .SendAsync("ReceiveGroupMessage", messageDto);
            }

            _logger.LogDebug("Emitted message sent event for message {MessageId}", eventData.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ChatMessageSentEvent for message {MessageId}", eventData.MessageId);
        }
    }

    private async Task HandleMessageEditedEvent(ChatMessageEditedEvent eventData)
    {
        try
        {
            var signalREvent = new
            {
                messageId = eventData.MessageId.ToString(),
                content = eventData.NewContent,
                editedAt = eventData.EditedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                isEdited = true,
                editReason = eventData.EditReason
            };

            await EmitToConversation(eventData.RecipientId, eventData.GroupId, "MessageEdited", signalREvent);
            _logger.LogDebug("Emitted MessageEdited event for message {MessageId}", eventData.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ChatMessageEditedEvent for message {MessageId}", eventData.MessageId);
        }
    }

    private async Task HandleMessageDeletedEvent(ChatMessageDeletedEvent eventData)
    {
        try
        {
            var signalREvent = new
            {
                messageId = eventData.MessageId.ToString(),
                isHardDelete = eventData.IsHardDelete,
                deletedAt = eventData.DeletedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                deletedBy = eventData.DeletedBy.ToString()
            };

            await EmitToConversation(eventData.RecipientId, eventData.GroupId, "MessageDeleted", signalREvent);
            _logger.LogDebug("Emitted MessageDeleted event for message {MessageId}", eventData.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ChatMessageDeletedEvent for message {MessageId}", eventData.MessageId);
        }
    }

    private async Task HandleMessageReadEvent(ChatMessageReadEvent eventData)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var chatService = scope.ServiceProvider.GetRequiredService<IChatService>();
            
            // Get message details to determine routing
            var messageDetails = await chatService.GetMessagesDetailsForEventAsync(eventData.MessageIds);
            
            // Group messages by conversation to minimize SignalR calls
            var directConversations = new Dictionary<string, List<Guid>>();
            var groupConversations = new Dictionary<Guid, List<Guid>>();

            foreach (var message in messageDetails)
            {
                if (message.ConversationType == "direct")
                {
                    var conversationId = GetDirectConversationId(message.SenderId, message.RecipientId);
                    if (!directConversations.ContainsKey(conversationId))
                        directConversations[conversationId] = new List<Guid>();
                    directConversations[conversationId].Add(message.Id);
                }
                else if (message.ConversationType == "group")
                {
                    if (!groupConversations.ContainsKey(message.GroupId))
                        groupConversations[message.GroupId] = new List<Guid>();
                    groupConversations[message.GroupId].Add(message.Id);
                }
            }

            // Emit events for direct conversations
            foreach (var kvp in directConversations)
            {
                var signalREvent = new
                {
                    messageIds = kvp.Value.Select(id => id.ToString()).ToList(),
                    readBy = eventData.ReadByUserId.ToString(),
                    readAt = eventData.ReadAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                };

                await _hubContext.Clients.Group($"conversation_{kvp.Key}")
                    .SendAsync("MessagesRead", signalREvent);
            }

            // Emit events for group conversations
            foreach (var kvp in groupConversations)
            {
                var signalREvent = new
                {
                    messageIds = kvp.Value.Select(id => id.ToString()).ToList(),
                    readBy = eventData.ReadByUserId.ToString(),
                    readAt = eventData.ReadAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                };

                await _hubContext.Clients.Group($"conversation_group_{kvp.Key}")
                    .SendAsync("MessagesRead", signalREvent);
            }

            _logger.LogDebug("Emitted MessagesRead event for {Count} messages", eventData.MessageIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ChatMessageReadEvent");
        }
    }

    // ===== REACTION EVENTS =====

    private async Task HandleReactionAddedEvent(MessageReactionAddedEvent eventData)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();

            var signalREvent = new
            {
                messageId = eventData.MessageId.ToString(),
                userId = eventData.ReactedBy.ToString(),
                userName = eventData.ReactedByName,
                reactionType = eventData.ReactionType.ToString(),
                reactionCounts = eventData.UpdatedReactionCounts.ToDictionary(
                    kvp => kvp.Key.ToString(), 
                    kvp => kvp.Value
                ),
                timestamp = eventData.ReactedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            await EmitToConversation(eventData.RecipientId, eventData.GroupId, "ReactionAdded", signalREvent);
            _logger.LogDebug("Emitted ReactionAdded event for message {MessageId}", eventData.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling MessageReactionAddedEvent for message {MessageId}", eventData.MessageId);
        }
    }

    private async Task HandleReactionRemovedEvent(MessageReactionRemovedEvent eventData)
    {
        try
        {
            var signalREvent = new
            {
                messageId = eventData.MessageId.ToString(),
                userId = eventData.RemovedBy.ToString(),
                reactionType = eventData.ReactionType.ToString(),
                reactionCounts = eventData.UpdatedReactionCounts.ToDictionary(
                    kvp => kvp.Key.ToString(), 
                    kvp => kvp.Value
                ),
                timestamp = eventData.RemovedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            await EmitToConversation(eventData.RecipientId, eventData.GroupId, "ReactionRemoved", signalREvent);
            _logger.LogDebug("Emitted ReactionRemoved event for message {MessageId}", eventData.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling MessageReactionRemovedEvent for message {MessageId}", eventData.MessageId);
        }
    }

    // ===== TYPING EVENTS =====

    private async Task HandleUserTypingEvent(UserTypingEvent eventData)
    {
        try
        {
            var signalREvent = new
            {
                userId = eventData.UserId.ToString(),
                userName = eventData.UserName,
                groupId = eventData.GroupId?.ToString(),
                recipientId = eventData.RecipientId?.ToString()
            };

            if (eventData.RecipientId.HasValue)
            {
                // Direct message typing
                await _hubContext.Clients.User(eventData.RecipientId.ToString())
                    .SendAsync("UserTyping", signalREvent);
            }
            else if (eventData.GroupId.HasValue)
            {
                // Group typing - send to everyone except the typer
                await _hubContext.Clients.GroupExcept($"conversation_group_{eventData.GroupId}", 
                    eventData.UserId.ToString())
                    .SendAsync("UserTyping", signalREvent);
            }

            _logger.LogDebug("Emitted UserTyping event for user {UserId}", eventData.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling UserTypingEvent for user {UserId}", eventData.UserId);
        }
    }

    private async Task HandleUserStoppedTypingEvent(UserStoppedTypingEvent eventData)
    {
        try
        {
            var signalREvent = new
            {
                userId = eventData.UserId.ToString(),
                groupId = eventData.GroupId?.ToString(),
                recipientId = eventData.RecipientId?.ToString()
            };

            if (eventData.RecipientId.HasValue)
            {
                // Direct message stopped typing
                await _hubContext.Clients.User(eventData.RecipientId.ToString())
                    .SendAsync("UserStoppedTyping", signalREvent);
            }
            else if (eventData.GroupId.HasValue)
            {
                // Group stopped typing
                await _hubContext.Clients.GroupExcept($"conversation_group_{eventData.GroupId}", 
                    eventData.UserId.ToString())
                    .SendAsync("UserStoppedTyping", signalREvent);
            }

            _logger.LogDebug("Emitted UserStoppedTyping event for user {UserId}", eventData.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling UserStoppedTypingEvent for user {UserId}", eventData.UserId);
        }
    }

    // ===== CONNECTION EVENTS =====

    private async Task HandleUserConnectedEvent(UserConnectedEvent eventData)
    {
        try
        {
            var signalREvent = new
            {
                userId = eventData.UserId.ToString(),
                connectedAt = eventData.ConnectedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            // Notify all users about online status change
            await _hubContext.Clients.All.SendAsync("UserOnline", eventData.UserId.ToString());
            _logger.LogDebug("Emitted UserOnline event for user {UserId}", eventData.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling UserConnectedEvent for user {UserId}", eventData.UserId);
        }
    }

    private async Task HandleUserDisconnectedEvent(UserDisconnectedEvent eventData)
    {
        try
        {
            var signalREvent = new
            {
                userId = eventData.UserId.ToString(),
                disconnectedAt = eventData.DisconnectedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            // Only notify if it's the last connection for this user
            if (eventData.IsLastConnection)
            {
                await _hubContext.Clients.All.SendAsync("UserOffline", eventData.UserId.ToString());
            }
            _logger.LogDebug("Emitted UserOffline event for user {UserId}", eventData.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling UserDisconnectedEvent for user {UserId}", eventData.UserId);
        }
    }

    // ===== HELPER METHODS =====

    private async Task EmitToConversation(Guid? recipientId, Guid? groupId, string eventName, object eventData)
    {
        if (recipientId.HasValue)
        {
            // Direct message - need to determine conversation ID
            // We'll send to both users since we don't know the sender here
            await _hubContext.Clients.User(recipientId.ToString())
                .SendAsync(eventName, eventData);
        }
        else if (groupId.HasValue)
        {
            // Group message
            await _hubContext.Clients.Group($"conversation_group_{groupId}")
                .SendAsync(eventName, eventData);
        }
    }

    private string GetDirectConversationId(Guid userId1, Guid userId2)
    {
        // Create consistent conversation ID regardless of order
        var ids = new[] { userId1.ToString(), userId2.ToString() }.OrderBy(x => x);
        return string.Join("_", ids);
    }
}