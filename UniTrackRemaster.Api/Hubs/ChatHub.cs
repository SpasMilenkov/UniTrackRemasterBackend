using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UniTrackRemaster.Api.Dto.Chat;
using UniTrackRemaster.Api.Dto.Chat.Events;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Services.User;

namespace UniTrackRemaster.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly IConnectionManager _connectionManager;
        private readonly IEventBus _eventBus;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(
            IChatService chatService,
            IUserService userService,
            IConnectionManager connectionManager,
            IEventBus eventBus,
            ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _userService = userService;
            _connectionManager = connectionManager;
            _eventBus = eventBus;
            _logger = logger;
        }

        // ========== CONNECTION METHODS ==========
        [Authorize(Policy = "SignalRService")]
        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId.HasValue)
            {
                // Add connection to manager (this will publish UserConnectedEvent)
                await _connectionManager.AddConnectionAsync(userId.Value, Context.ConnectionId);

                // Add user to their personal group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

                // Add user to institution groups
                try
                {
                    var userInstitutions = await _userService.GetUserInstitutionsAsync(userId.Value);
                    foreach (var institution in userInstitutions)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"institution_{institution.Id}");
                    }
                    
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding user to institution groups");
                }

                _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId.HasValue)
            {
                // Remove connection (this will publish UserDisconnectedEvent)
                await _connectionManager.RemoveConnectionAsync(userId.Value, Context.ConnectionId);

                _logger.LogInformation("User {UserId} disconnected", userId);
            }

            if (exception != null)
            {
                _logger.LogError(exception, "Connection disconnected with error");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ========== MESSAGE METHODS ==========
        public async Task<ChatMessageDto> SendDirectMessage(SendDirectMessageDto dto)
        {
            try
            {
                var senderId = GetUserId() ?? throw new HubException("User not authenticated");

                // Send message through service
                var messageDto = await _chatService.SendDirectMessageAsync(senderId, dto);
        
                // Immediate delivery confirmation to sender
                await Clients.Caller.SendAsync("MessageDelivered", new
                {
                    MessageId = messageDto.Id,
                    Status = "Delivered",
                    DeliveredAt = DateTime.UtcNow
                });

                // Message will be sent to recipient via EventBus -> ChatSignalREventHandler
                _logger.LogInformation($"Message sent with text {messageDto.Content}");
        
                return messageDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending direct message");
                throw new HubException("Failed to send message");
            }
        }

        public async Task<ChatMessageDto> SendGroupMessage(SendGroupMessageDto dto)
        {
            try
            {
                var senderId = GetUserId() ?? throw new HubException("Not authenticated");

                // Send message through service (this will publish ChatMessageSentEvent)
                var messageDto = await _chatService.SendGroupMessageAsync(senderId, dto);

                await Clients.Caller.SendAsync("MessageDelivered", new
                {
                    MessageId = messageDto.Id,
                    Status = "Delivered",
                    DeliveredAt = DateTime.UtcNow
                });
                _logger.LogInformation($"Group Message sent with text {messageDto.Content}");

                return messageDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending group message");
                throw new HubException("Failed to send message");
            }
        }


        public async Task MarkMessagesAsRead(MarkMessagesReadDto dto)
        {
            try
            {
                var userId = GetUserId() ?? throw new HubException("User not authenticated");

                // Mark messages as read (this will publish ChatMessageReadEvent)
                await _chatService.MarkMessagesAsReadAsync(userId, dto.MessageIds);

                // EventBus handler will emit SignalR events - no need to do it here
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read");
                throw new HubException("Failed to mark messages as read");
            }
        }


        // ========== MESSAGE EDITING ==========
        public async Task<ChatMessageDto> EditMessage(Guid messageId, EditMessageDto dto)
        {
            try
            {
                var userId = GetUserId() ?? throw new HubException("User not authenticated");

                // Edit message through service (this will publish ChatMessageEditedEvent)
                var editedMessage = await _chatService.EditMessageAsync(messageId, userId, dto);

                // EventBus handler will emit SignalR events - no need to do it here
                _logger.LogInformation("User {UserId} edited message {MessageId}", userId, messageId);
                return editedMessage;
            }
            catch (NotFoundException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (ForbiddenException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing message {MessageId}", messageId);
                throw new HubException("Failed to edit message");
            }
        }

        public async Task<bool> CanEditMessage(Guid messageId)
        {
            try
            {
                var userId = GetUserId() ?? throw new HubException("User not authenticated");
                return await _chatService.CanEditMessageAsync(messageId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking edit permissions for message {MessageId}", messageId);
                return false;
            }
        }

        // ========== MESSAGE DELETION ==========
        public async Task DeleteMessage(Guid messageId, bool isHardDelete = false)
        {
            try
            {
                var userId = GetUserId() ?? throw new HubException("User not authenticated");

                var dto = new DeleteMessageDto(isHardDelete);
                var success = await _chatService.DeleteMessageAsync(messageId, userId, dto);

                if (!success)
                {
                    throw new HubException("Failed to delete message");
                }

                // EventBus handler will emit SignalR events - no need to do it here
                _logger.LogInformation("User {UserId} deleted message {MessageId} (hard: {IsHardDelete})",
                    userId, messageId, isHardDelete);
            }
            catch (NotFoundException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (ForbiddenException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
                throw new HubException("Failed to delete message");
            }
        }

        public async Task<bool> CanDeleteMessage(Guid messageId)
        {
            try
            {
                var userId = GetUserId() ?? throw new HubException("User not authenticated");
                return await _chatService.CanDeleteMessageAsync(messageId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking delete permissions for message {MessageId}", messageId);
                return false;
            }
        }

        // ========== REACTIONS ==========
        public async Task<MessageReactionsSummaryDto> AddReaction(Guid messageId, AddReactionDto dto)
        {
            try
            {
                var userId = GetUserId() ?? throw new HubException("User not authenticated");

                // Add reaction through service (this will publish MessageReactionAddedEvent)
                var result = await _chatService.AddReactionAsync(messageId, userId, dto);

                // EventBus handler will emit SignalR events - no need to do it here
                _logger.LogInformation("User {UserId} added reaction {ReactionType} to message {MessageId}",
                    userId, dto.ReactionType, messageId);
                return result;
            }
            catch (NotFoundException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (BadRequestException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding reaction to message {MessageId}", messageId);
                throw new HubException("Failed to add reaction");
            }
        }



        public async Task<MessageReactionsSummaryDto> RemoveReaction(Guid messageId, AddReactionDto dto)
        {
            try
            {
                var userId = GetUserId() ?? throw new HubException("User not authenticated");

                // Remove reaction through service (this will publish MessageReactionRemovedEvent)
                var result = await _chatService.RemoveReactionAsync(messageId, userId, dto);

                // EventBus handler will emit SignalR events - no need to do it here
                _logger.LogInformation("User {UserId} removed reaction {ReactionType} from message {MessageId}",
                    userId, dto.ReactionType, messageId);
                return result;
            }
            catch (NotFoundException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing reaction from message {MessageId}", messageId);
                throw new HubException("Failed to remove reaction");
            }
        }

        public async Task<Dictionary<ReactionType, int>> GetReactionCounts(Guid messageId)
        {
            try
            {
                return await _chatService.GetReactionCountsAsync(messageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reaction counts for message {MessageId}", messageId);
                return new Dictionary<ReactionType, int>();
            }
        }

        // ========== REPLIES ==========
        public async Task<GetMessageRepliesResponseDto> GetMessageReplies(Guid parentMessageId, int page = 1, int pageSize = 20)
        {
            try
            {
                var request = new GetMessageRepliesDto(parentMessageId, page, pageSize);
                return await _chatService.GetMessageRepliesAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting replies for message {ParentMessageId}", parentMessageId);
                throw new HubException("Failed to get message replies");
            }
        }

        // ========== TYPING METHODS ==========
        public async Task StartTyping(TypingIndicatorDto dto)
        {
            try
            {
                var userId = GetUserId() ?? throw new HubException("User not authenticated");
                var user = await _userService.GetUserDetailsAsync(userId);

                // Publish typing event (EventBus handler will emit SignalR events)
                await _eventBus.PublishAsync(new UserTypingEvent
                {
                    UserId = userId,
                    UserName = $"{user.FirstName} {user.LastName}",
                    RecipientId = dto.RecipientId,
                    GroupId = dto.GroupId,
                    GroupType = dto.GroupType
                });

                // Set typing timeout
                await _connectionManager.SetTypingTimeoutAsync(userId, dto.RecipientId, dto.GroupId, dto.GroupType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending typing indicator");
            }
        }

        public async Task StopTyping(TypingIndicatorDto dto)
        {
            try
            {
                var userId = GetUserId() ?? throw new HubException("User not authenticated");

                // Publish stop typing event (EventBus handler will emit SignalR events)
                await _eventBus.PublishAsync(new UserStoppedTypingEvent
                {
                    UserId = userId,
                    RecipientId = dto.RecipientId,
                    GroupId = dto.GroupId,
                    GroupType = dto.GroupType
                });

                // Clear typing timeout
                await _connectionManager.ClearTypingTimeoutAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping typing indicator");
            }
        }
        // ========== GROUP METHODS ==========
        public async Task<List<Guid>> GetOnlineUsers()
        {
            try
            {
                return await _connectionManager.GetOnlineUsersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online users");
                return new List<Guid>();
            }
        }

        public async Task JoinConversation(JoinConversationDto dto)
        {
            try
            {
                var userId = GetUserId() ?? throw new HubException("User not authenticated");

                switch (dto.ConversationType)
                {
                    case "direct" when dto.OtherUserId.HasValue:
                    {
                        var conversationId = GetDirectConversationId(userId, dto.OtherUserId.Value);
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");

                        _logger.LogInformation("User {UserId} joined direct conversation with {OtherUserId}", userId, dto.OtherUserId);
                        break;
                    }
                    case "group" when dto.GroupId.HasValue:
                    {
                        // Verify access
                        var hasAccess = await _userService.UserHasAccessToGroupAsync(userId, dto.GroupId.Value, dto.GroupType);
                        if (hasAccess)
                        {
                            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_group_{dto.GroupId}");
                            _logger.LogInformation("User {UserId} joined group conversation {GroupId}", userId, dto.GroupId);
                        }
                        else
                        {
                            throw new HubException("You don't have access to this group");
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining conversation");
                throw new HubException("Failed to join conversation");
            }
        }

        public async Task LeaveConversation(JoinConversationDto dto)
        {
            try
            {
                var userId = GetUserId() ?? throw new HubException("User not authenticated");

                if (dto.ConversationType == "direct" && dto.OtherUserId.HasValue)
                {
                    var conversationId = GetDirectConversationId(userId, dto.OtherUserId.Value);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
                }
                else if (dto.ConversationType == "group" && dto.GroupId.HasValue)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_group_{dto.GroupId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving conversation");
            }
        }

        // ========== HELPER METHODS ==========
        private Guid? GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return null;
            }
            return userId;
        }

        private string GetDirectConversationId(Guid userId1, Guid userId2)
        {
            // Create consistent conversation ID regardless of order
            var ids = new[] { userId1.ToString(), userId2.ToString() }.OrderBy(x => x);
            return string.Join("_", ids);
        }
    }
}