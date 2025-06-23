using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Chat;
using UniTrackRemaster.Api.Dto.Chat.Events;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Chat;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Services.User;

namespace UniTrackRemaster.Services.Messaging;

public class ChatService : IChatService
{
    private readonly UniTrackDbContext _context;
    private readonly IUserService _userService;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        UniTrackDbContext context,
        IUserService userService,
        IEventBus eventBus,
        ILogger<ChatService> logger)
    {
        _context = context;
        _userService = userService;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<GetConversationsResponseDto> GetConversationsAsync(Guid userId)
    {
        var conversations = new List<ConversationDto>();

        // Get direct message conversations
        var directConversations = await GetDirectConversationsAsync(userId);
        conversations.AddRange(directConversations);

        // Get group conversations
        var groupConversations = await GetGroupConversationsAsync(userId);
        conversations.AddRange(groupConversations);

        // Sort by last activity
        conversations = conversations
            .OrderByDescending(c => c.LastActivity ?? DateTime.MinValue)
            .ToList();

        return new GetConversationsResponseDto(conversations, conversations.Count);
    }

    public async Task<ConversationDto?> GetConversationAsync(
        Guid userId,
        string conversationType,
        Guid? otherUserId = null,
        Guid? groupId = null)
    {
        if (conversationType == "direct" && otherUserId.HasValue)
        {
            var otherUser = await _userService.GetUserDetailsAsync(otherUserId.Value);
            var lastMessage = await GetLastDirectMessageAsync(userId, otherUserId.Value);
            var unreadCount = await GetDirectUnreadCountAsync(userId, otherUserId.Value);

            return new ConversationDto(
                Id: otherUserId.Value,
                Type: "direct",
                Name: $"{otherUser.FirstName} {otherUser.LastName}",
                Avatar: otherUser.ProfileImageUrl,
                OtherUserId: otherUserId,
                GroupId: null,
                LastMessage: lastMessage != null ? MapToDto(lastMessage, userId) : null,
                UnreadCount: unreadCount,
                LastActivity: lastMessage?.SentAt,
                OnlineUserIds: new List<Guid>()
            );
        }
        else if (conversationType == "group" && groupId.HasValue)
        {
            var institution = (await _userService.GetUserInstitutionsAsync(userId))
                .FirstOrDefault(i => i.Id == groupId);

            if (institution == null)
                return null;

            var lastMessage = await GetLastGroupMessageAsync(groupId.Value);

            return new ConversationDto(
                Id: institution.Id,
                Type: "group",
                Name: institution.Name,
                Avatar: institution.LogoUrl,
                OtherUserId: null,
                GroupId: institution.Id,
                LastMessage: lastMessage != null ? MapToDto(lastMessage, userId) : null,
                UnreadCount: 0,
                LastActivity: lastMessage?.SentAt,
                OnlineUserIds: new List<Guid>()
            );
        }

        return null;
    }
    public async Task<dynamic> GetMessageDetailsForEventAsync(Guid messageId)
    {
        var message = await _context.ChatMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
            throw new NotFoundException("Message not found");

        return new
        {
            Id = message.Id,
            SenderId = message.SenderId,
            RecipientId = message.RecipientId,
            GroupId = message.GroupId,
            ConversationType = message.MessageType == MessageType.Direct ? "direct" : "group"
        };
    }

    public async Task<List<dynamic>> GetMessagesDetailsForEventAsync(List<Guid> messageIds)
    {
        var messages = await _context.ChatMessages
            .AsNoTracking()
            .Where(m => messageIds.Contains(m.Id))
            .ToListAsync();

        return messages.Select(message => new
        {
            Id = message.Id,
            SenderId = message.SenderId,
            RecipientId = message.RecipientId,
            GroupId = message.GroupId,
            ConversationType = message.MessageType == MessageType.Direct ? "direct" : "group"
        }).Cast<dynamic>().ToList();
    }
    public async Task<GetMessagesResponseDto> GetMessagesAsync(Guid userId, GetMessagesDto dto)
    {
        IQueryable<ChatMessage> query = _context.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.ParentMessage)
                .ThenInclude(pm => pm!.Sender);

        // Apply conversation filter
        if (dto.ConversationType == "direct" && dto.OtherUserId.HasValue)
        {
            query = query.Where(m =>
                m.MessageType == MessageType.Direct &&
                ((m.SenderId == userId && m.RecipientId == dto.OtherUserId) ||
                 (m.SenderId == dto.OtherUserId && m.RecipientId == userId)));
        }
        else if (dto.ConversationType == "group" && dto.GroupId.HasValue)
        {
            // Verify access
            var hasAccess = await _userService.UserHasAccessToGroupAsync(userId, dto.GroupId.Value, "institution");
            if (!hasAccess)
            {
                throw new ForbiddenException("You don't have access to this group");
            }

            query = query.Where(m => m.GroupId == dto.GroupId &&
                (m.MessageType == MessageType.Institution || m.MessageType == MessageType.Class));
        }
        else
        {
            throw new BadRequestException("Invalid conversation type or missing parameters");
        }

        // Apply time filter
        if (dto.Before.HasValue)
        {
            query = query.Where(m => m.SentAt < dto.Before.Value);
        }

        var totalCount = await query.CountAsync();

        // Get paginated messages
        var messages = await query
            .OrderByDescending(m => m.SentAt)
            .Skip((dto.Page - 1) * dto.PageSize)
            .Take(dto.PageSize)
            .ToListAsync();

        // Map to DTOs and reverse to get chronological order
        var messageDtos = messages
            .Select(m => MapToDto(m, userId))
            .Reverse()
            .ToList();

        return new GetMessagesResponseDto(
            Messages: messageDtos,
            TotalCount: totalCount,
            HasMore: totalCount > dto.Page * dto.PageSize
        );
    }

    public async Task<ChatMessageDto> SendDirectMessageAsync(Guid senderId, SendDirectMessageDto dto)
    {
        // Validate recipient exists
        var recipient = await _userService.GetUserDetailsAsync(dto.RecipientId);
        if (recipient == null)
        {
            throw new NotFoundException("Recipient not found");
        }

        // Create message
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            RecipientId = dto.RecipientId,
            Content = dto.Content,
            MessageType = MessageType.Direct,
            Status = MessageStatus.Sent,
            SentAt = DateTime.UtcNow,
            ParentMessageId = dto.ParentMessageId
        };

        // Validate parent message if provided
        if (dto.ParentMessageId.HasValue)
        {
            var parentExists = await _context.ChatMessages
                .AnyAsync(m => m.Id == dto.ParentMessageId.Value);

            if (!parentExists)
            {
                throw new NotFoundException("Parent message not found");
            }
        }

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        // Reload with includes
        message = await _context.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.ParentMessage)
                .ThenInclude(pm => pm!.Sender)
            .FirstAsync(m => m.Id == message.Id);

        // Publish event
        await _eventBus.PublishAsync(new ChatMessageSentEvent
        {
            MessageId = message.Id,
            SenderId = senderId,
            RecipientId = dto.RecipientId,
            Content = dto.Content,
            MessageType = MessageType.Direct,
            SentAt = message.SentAt
        });

        _logger.LogInformation("Direct message sent from {SenderId} to {RecipientId}", senderId, dto.RecipientId);

        return MapToDto(message, senderId);
    }

    public async Task<ChatMessageDto> SendGroupMessageAsync(Guid senderId, SendGroupMessageDto dto)
    {
        // Verify user has access to group
        var hasAccess = await _userService.UserHasAccessToGroupAsync(senderId, dto.GroupId, dto.GroupType);
        if (!hasAccess)
        {
            throw new ForbiddenException("You don't have access to this group");
        }

        // Create message
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            GroupId = dto.GroupId,
            Content = dto.Content,
            MessageType = dto.GroupType == "institution" ? MessageType.Institution : MessageType.Class,
            Status = MessageStatus.Sent,
            SentAt = DateTime.UtcNow,
            ParentMessageId = dto.ParentMessageId
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        // Reload with includes
        message = await _context.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.ParentMessage)
                .ThenInclude(pm => pm!.Sender)
            .FirstAsync(m => m.Id == message.Id);

        // Publish event
        await _eventBus.PublishAsync(new ChatMessageSentEvent
        {
            MessageId = message.Id,
            SenderId = senderId,
            GroupId = dto.GroupId,
            Content = dto.Content,
            MessageType = message.MessageType,
            SentAt = message.SentAt
        });

        _logger.LogInformation("Group message sent to {GroupType} {GroupId} by {SenderId}", dto.GroupType, dto.GroupId, senderId);
        var mappedMessage = MapToDto(message, senderId);
        mappedMessage.IsOwnMessage = false;
        return mappedMessage;
    }

    public async Task<List<ChatMessageDto>> SearchMessagesAsync(Guid userId, SearchMessagesDto dto)
    {
        var query = _context.ChatMessages
            .Include(m => m.Sender)
            .Where(m => EF.Functions.ILike(m.Content, $"%{dto.SearchTerm}%"));

        // Filter by user's accessible messages
        var userInstitutionIds = (await _userService.GetUserInstitutionsAsync(userId))
            .Select(i => i.Id)
            .ToList();

        query = query.Where(m =>
            (m.SenderId == userId || m.RecipientId == userId) ||
            (m.GroupId != null && userInstitutionIds.Contains(m.GroupId.Value))
        );

        // Filter by specific conversation if provided
        if (dto.ConversationId.HasValue)
        {
            if (dto.ConversationType == "direct")
            {
                query = query.Where(m =>
                    m.MessageType == MessageType.Direct &&
                    ((m.SenderId == userId && m.RecipientId == dto.ConversationId) ||
                     (m.SenderId == dto.ConversationId && m.RecipientId == userId)));
            }
            else if (dto.ConversationType == "group")
            {
                query = query.Where(m => m.GroupId == dto.ConversationId);
            }
        }

        var messages = await query
            .OrderByDescending(m => m.SentAt)
            .Take(50)
            .ToListAsync();

        return messages.Select(m => MapToDto(m, userId)).ToList();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _context.ChatMessages
            .Where(m => m.RecipientId == userId && m.Status != MessageStatus.Read)
            .CountAsync();
    }

    public async Task MarkMessagesAsReadAsync(Guid userId, List<Guid> messageIds)
    {
        var messages = await _context.ChatMessages
            .Where(m => messageIds.Contains(m.Id) &&
                       m.RecipientId == userId &&
                       m.Status != MessageStatus.Read)
            .ToListAsync();

        if (!messages.Any())
            return;

        var now = DateTime.UtcNow;
        foreach (var message in messages)
        {
            message.Status = MessageStatus.Read;
            message.ReadAt = now;
        }

        await _context.SaveChangesAsync();

        // Publish event
        await _eventBus.PublishAsync(new ChatMessageReadEvent
        {
            MessageIds = messages.Select(m => m.Id).ToList(),
            ReadByUserId = userId,
            ReadAt = now
        });

        _logger.LogInformation("Marked {Count} messages as read for user {UserId}", messages.Count, userId);
    }

    public async Task<List<Guid>> GetMessageRecipientsAsync(List<Guid> messageIds)
    {
        return await _context.ChatMessages
            .Where(m => messageIds.Contains(m.Id))
            .Select(m => m.SenderId)
            .Distinct()
            .ToListAsync();
    }

    public async Task UpdateMessageStatusAsync(Guid messageId, MessageStatus status)
    {
        var message = await _context.ChatMessages.FindAsync(messageId);
        if (message != null)
        {
            message.Status = status;
            if (status == MessageStatus.Delivered)
            {
                message.DeliveredAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }
    }
    public async Task<bool> UserCanAccessMessageAsync(Guid userId, Guid messageId)
    {
        var message = await _context.ChatMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
            return false;

        // Direct message: user must be sender or recipient
        if (message.MessageType == MessageType.Direct)
        {
            return message.SenderId == userId || message.RecipientId == userId;
        }

        // Group message: user must have access to the group
        if (message.GroupId.HasValue)
        {
            return await _userService.UserHasAccessToGroupAsync(userId, message.GroupId.Value, "institution");
        }

        return false;
    }

    // Private helper methods
    private async Task<List<ConversationDto>> GetDirectConversationsAsync(Guid userId)
    {
        var directConversations = await _context.ChatMessages
            .Where(m => (m.SenderId == userId || m.RecipientId == userId) &&
                       m.MessageType == MessageType.Direct)
            .GroupBy(m => m.SenderId == userId ? m.RecipientId : m.SenderId)
            .Select(g => new
            {
                OtherUserId = g.Key,
                LastMessage = g.OrderByDescending(m => m.SentAt).FirstOrDefault(),
                UnreadCount = g.Count(m => m.RecipientId == userId && m.Status != MessageStatus.Read)
            })
            .ToListAsync();

        var conversations = new List<ConversationDto>();

        foreach (var conv in directConversations.Where(c => c.OtherUserId.HasValue))
        {
            try
            {
                var otherUser = await _userService.GetUserDetailsAsync(conv.OtherUserId!.Value);

                // Load the last message with includes if it exists
                ChatMessage? lastMessageWithIncludes = null;
                if (conv.LastMessage != null)
                {
                    lastMessageWithIncludes = await _context.ChatMessages
                        .Include(m => m.Sender)
                        .FirstOrDefaultAsync(m => m.Id == conv.LastMessage.Id);
                }

                conversations.Add(new ConversationDto(
                    Id: conv.OtherUserId!.Value,
                    Type: "direct",
                    Name: $"{otherUser.FirstName} {otherUser.LastName}",
                    Avatar: otherUser.ProfileImageUrl,
                    OtherUserId: conv.OtherUserId,
                    GroupId: null,
                    LastMessage: lastMessageWithIncludes != null ? MapToDto(lastMessageWithIncludes, userId) : null,
                    UnreadCount: conv.UnreadCount,
                    LastActivity: conv.LastMessage?.SentAt,
                    OnlineUserIds: new List<Guid>()
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading conversation for user {OtherUserId}", conv.OtherUserId);
            }
        }

        return conversations;
    }

    private async Task<List<ConversationDto>> GetGroupConversationsAsync(Guid userId)
    {
        var conversations = new List<ConversationDto>();
        var userInstitutions = await _userService.GetUserInstitutionsAsync(userId);

        foreach (var institution in userInstitutions)
        {
            var lastMessage = await _context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => m.GroupId == institution.Id &&
                           m.MessageType == MessageType.Institution)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();

            conversations.Add(new ConversationDto(
                Id: institution.Id,
                Type: "group",
                Name: institution.Name,
                Avatar: institution.LogoUrl,
                OtherUserId: null,
                GroupId: institution.Id,
                LastMessage: lastMessage != null ? MapToDto(lastMessage, userId) : null,
                UnreadCount: 0,
                LastActivity: lastMessage?.SentAt,
                OnlineUserIds: new List<Guid>()
            ));
        }

        return conversations;
    }

    private async Task<ChatMessage?> GetLastDirectMessageAsync(Guid userId, Guid otherUserId)
    {
        return await _context.ChatMessages
            .Include(m => m.Sender)
            .Where(m => m.MessageType == MessageType.Direct &&
                       ((m.SenderId == userId && m.RecipientId == otherUserId) ||
                        (m.SenderId == otherUserId && m.RecipientId == userId)))
            .OrderByDescending(m => m.SentAt)
            .FirstOrDefaultAsync();
    }

    private async Task<ChatMessage?> GetLastGroupMessageAsync(Guid groupId)
    {
        return await _context.ChatMessages
            .Include(m => m.Sender)
            .Where(m => m.GroupId == groupId)
            .OrderByDescending(m => m.SentAt)
            .FirstOrDefaultAsync();
    }

    private async Task<int> GetDirectUnreadCountAsync(Guid userId, Guid otherUserId)
    {
        return await _context.ChatMessages
            .Where(m => m.MessageType == MessageType.Direct &&
                       m.SenderId == otherUserId &&
                       m.RecipientId == userId &&
                       m.Status != MessageStatus.Read)
            .CountAsync();
    }
    public async Task<ChatMessage?> GetMessageByIdAsync(Guid messageId)
    {
        return await _context.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.ParentMessage)
                .ThenInclude(pm => pm!.Sender)
            .FirstOrDefaultAsync(m => m.Id == messageId);
    }
    // ========== MESSAGE EDITING ==========
    public async Task<ChatMessageDto> EditMessageAsync(Guid messageId, Guid userId, EditMessageDto dto)
    {
        var message = await _context.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.EditHistory)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
        {
            throw new NotFoundException("Message not found");
        }

        if (!await CanEditMessageAsync(messageId, userId))
        {
            throw new ForbiddenException("You cannot edit this message");
        }

        // Store original content in edit history
        var editHistory = new MessageEditHistory
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            PreviousContent = message.Content,
            NewContent = dto.Content,
            EditedAt = DateTime.UtcNow,
            EditReason = dto.EditReason
        };

        // Update message
        message.OriginalContent ??= message.Content; // Store original content if first edit
        message.Content = dto.Content;
        message.EditedAt = DateTime.UtcNow;
        message.Status = MessageStatus.Edited;

        _context.MessageEditHistories.Add(editHistory);
        await _context.SaveChangesAsync();

        // Reload with all includes
        message = await GetMessageWithAllIncludesAsync(messageId);

        // Publish event
        await _eventBus.PublishAsync(new ChatMessageEditedEvent
        {
            MessageId = messageId,
            SenderId = message.SenderId,
            RecipientId = message.RecipientId,
            GroupId = message.GroupId,
            NewContent = dto.Content,
            EditedAt = message.EditedAt!.Value,
            EditReason = dto.EditReason,
            EditCount = message.EditHistory?.Count ?? 0
        });

        _logger.LogInformation("Message {MessageId} edited by user {UserId}", messageId, userId);

        return MapToDto(message, userId);
    }

    public async Task<bool> CanEditMessageAsync(Guid messageId, Guid userId)
    {
        var message = await _context.ChatMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null || message.IsDeleted)
            return false;

        // Only sender can edit their own messages
        if (message.SenderId != userId)
            return false;

        return true;
    }

    public async Task<GetMessageEditHistoryResponseDto> GetMessageEditHistoryAsync(GetMessageEditHistoryDto request)
    {
        var editHistory = await _context.MessageEditHistories
            .Where(h => h.MessageId == request.MessageId)
            .OrderByDescending(h => h.EditedAt)
            .ToListAsync();

        var historyDtos = editHistory.Select(h => new MessageEditHistoryDto(
            h.Id,
            h.PreviousContent,
            h.NewContent,
            h.EditedAt,
            h.EditReason
        )).ToList();

        return new GetMessageEditHistoryResponseDto(historyDtos, editHistory.Count);
    }

    // ========== MESSAGE DELETION ==========
    public async Task<bool> DeleteMessageAsync(Guid messageId, Guid userId, DeleteMessageDto dto)
    {
        var message = await _context.ChatMessages
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
        {
            throw new NotFoundException("Message not found");
        }

        if (!await CanDeleteMessageAsync(messageId, userId))
        {
            throw new ForbiddenException("You cannot delete this message");
        }

        if (dto.IsHardDelete)
        {
            // Hard delete - remove completely (admin only)
            _context.ChatMessages.Remove(message);
        }
        else
        {
            // Soft delete - mark as deleted
            message.IsDeleted = true;
            message.DeletedAt = DateTime.UtcNow;
            message.DeletedBy = userId;
            message.Content = "[Message deleted]";
            message.Status = MessageStatus.Deleted;
        }

        await _context.SaveChangesAsync();

        // Publish event
        await _eventBus.PublishAsync(new ChatMessageDeletedEvent
        {
            MessageId = messageId,
            SenderId = message.SenderId,
            RecipientId = message.RecipientId,
            GroupId = message.GroupId,
            DeletedBy = userId,
            DeletedAt = DateTime.UtcNow,
            IsHardDelete = dto.IsHardDelete
        });

        _logger.LogInformation("Message {MessageId} {DeleteType} deleted by user {UserId}",
            messageId, dto.IsHardDelete ? "hard" : "soft", userId);

        return true;
    }

    public async Task<bool> CanDeleteMessageAsync(Guid messageId, Guid userId)
    {
        var message = await _context.ChatMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null || message.IsDeleted)
            return false;

        // Check if user is the sender
        if (message.SenderId == userId)
            return true;

        // Check if user is admin
        var isAdmin = await _userService.IsUserAdminAsync(userId);
        return isAdmin;
    }

    // ========== REACTIONS ==========
    public async Task<MessageReactionsSummaryDto> AddReactionAsync(Guid messageId, Guid userId, AddReactionDto dto)
    {
        var message = await _context.ChatMessages
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
        {
            throw new NotFoundException("Message not found");
        }

        if (message.IsDeleted)
        {
            throw new BadRequestException("Cannot react to deleted message");
        }

        // Check if user already has this reaction
        var existingReaction = await _context.MessageReactions
            .FirstOrDefaultAsync(r => r.MessageId == messageId &&
                                     r.UserId == userId &&
                                     r.ReactionType == dto.ReactionType);

        if (existingReaction != null)
        {
            throw new BadRequestException("You have already added this reaction");
        }

        // Remove any existing reaction from this user (if you want only one reaction per user)
        var userExistingReaction = await _context.MessageReactions
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId);

        if (userExistingReaction != null)
        {
            _context.MessageReactions.Remove(userExistingReaction);
        }

        // Add new reaction
        var reaction = new MessageReaction
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            UserId = userId,
            ReactionType = dto.ReactionType,
            ReactedAt = DateTime.UtcNow
        };

        _context.MessageReactions.Add(reaction);
        await _context.SaveChangesAsync();

        // Get updated reaction counts
        var reactionCounts = await GetReactionCountsAsync(messageId);
        var userReactions = await GetUserReactionsAsync(messageId, userId);

        // Get user details for event
        var user = await _userService.GetUserDetailsAsync(userId);

        // Publish event
        await _eventBus.PublishAsync(new MessageReactionAddedEvent
        {
            MessageId = messageId,
            SenderId = message.SenderId,
            RecipientId = message.RecipientId,
            GroupId = message.GroupId,
            ReactedBy = userId,
            ReactedByName = $"{user.FirstName} {user.LastName}",
            ReactionType = dto.ReactionType,
            ReactedAt = reaction.ReactedAt,
            UpdatedReactionCounts = reactionCounts
        });

        _logger.LogInformation("User {UserId} added reaction {ReactionType} to message {MessageId}",
            userId, dto.ReactionType, messageId);

        return new MessageReactionsSummaryDto(messageId, reactionCounts, userReactions);
    }

    public async Task<MessageReactionsSummaryDto> RemoveReactionAsync(Guid messageId, Guid userId, AddReactionDto dto)
    {
        var reaction = await _context.MessageReactions
            .FirstOrDefaultAsync(r => r.MessageId == messageId &&
                                     r.UserId == userId &&
                                     r.ReactionType == dto.ReactionType);

        if (reaction == null)
        {
            throw new NotFoundException("Reaction not found");
        }

        var message = await _context.ChatMessages
            .FirstOrDefaultAsync(m => m.Id == messageId);

        _context.MessageReactions.Remove(reaction);
        await _context.SaveChangesAsync();

        // Get updated reaction counts
        var reactionCounts = await GetReactionCountsAsync(messageId);
        var userReactions = await GetUserReactionsAsync(messageId, userId);

        // Publish event
        await _eventBus.PublishAsync(new MessageReactionRemovedEvent
        {
            MessageId = messageId,
            SenderId = message!.SenderId,
            RecipientId = message.RecipientId,
            GroupId = message.GroupId,
            RemovedBy = userId,
            ReactionType = dto.ReactionType,
            RemovedAt = DateTime.UtcNow,
            UpdatedReactionCounts = reactionCounts
        });

        _logger.LogInformation("User {UserId} removed reaction {ReactionType} from message {MessageId}",
            userId, dto.ReactionType, messageId);

        return new MessageReactionsSummaryDto(messageId, reactionCounts, userReactions);
    }

    public async Task<GetMessageReactionsResponseDto> GetMessageReactionsAsync(GetMessageReactionsDto request)
    {
        var reactions = await _context.MessageReactions
            .Include(r => r.User)
            .Where(r => r.MessageId == request.MessageId)
            .OrderByDescending(r => r.ReactedAt)
            .ToListAsync();

        var reactionDtos = reactions.Select(r => new ReactionDto(
            r.Id,
            r.UserId,
            $"{r.User.FirstName} {r.User.LastName}",
            r.ReactionType,
            r.ReactedAt
        )).ToList();

        var reactionCounts = reactions
            .GroupBy(r => r.ReactionType)
            .ToDictionary(g => g.Key, g => g.Count());

        return new GetMessageReactionsResponseDto(reactionDtos, reactionCounts);
    }

    public async Task<Dictionary<ReactionType, int>> GetReactionCountsAsync(Guid messageId)
    {
        return await _context.MessageReactions
            .Where(r => r.MessageId == messageId)
            .GroupBy(r => r.ReactionType)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    private async Task<List<ReactionType>> GetUserReactionsAsync(Guid messageId, Guid userId)
    {
        return await _context.MessageReactions
            .Where(r => r.MessageId == messageId && r.UserId == userId)
            .Select(r => r.ReactionType)
            .ToListAsync();
    }

    // ========== REPLIES ==========
    public async Task<GetMessageRepliesResponseDto> GetMessageRepliesAsync(GetMessageRepliesDto request)
    {
        var query = _context.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.Reactions)
            .Where(m => m.ParentMessageId == request.ParentMessageId && !m.IsDeleted);

        var totalCount = await query.CountAsync();

        var replies = await query
            .OrderBy(m => m.SentAt) // Replies in chronological order
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        // Get current user ID for mapping (you might need to pass this as parameter)
        var replyDtos = new List<ChatMessageDto>();
        foreach (var reply in replies)
        {
            var replyDto = await MapToDtoWithReactionsAsync(reply, Guid.Empty); // You'll need to pass userId
            replyDtos.Add(replyDto);
        }

        return new GetMessageRepliesResponseDto(
            replyDtos,
            totalCount,
            totalCount > request.Page * request.PageSize
        );
    }

    public async Task<ChatMessageDto> MapToDtoWithReactionsAsync(ChatMessage message, Guid currentUserId)
    {
        var reactionCounts = await GetReactionCountsAsync(message.Id);
        var userReactions = await GetUserReactionsAsync(message.Id, currentUserId);
        var replyCount = await _context.ChatMessages
            .CountAsync(m => m.ParentMessageId == message.Id && !m.IsDeleted);
        var editCount = await _context.MessageEditHistories
            .CountAsync(h => h.MessageId == message.Id);

        return new ChatMessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = message.Sender != null
                ? $"{message.Sender.FirstName} {message.Sender.LastName}"
                : "Unknown User",
            SenderAvatar = message.Sender?.AvatarUrl,
            RecipientId = message.RecipientId,
            GroupId = message.GroupId,
            Content = message.Content,
            MessageType = message.MessageType,
            Status = message.Status,
            SentAt = message.SentAt,
            DeliveredAt = message.DeliveredAt,
            ReadAt = message.ReadAt,
            ParentMessageId = message.ParentMessageId,
            AttachmentUrl = message.AttachmentUrl,
            AttachmentType = message.AttachmentType,
            IsOwnMessage = message.SenderId == currentUserId,

            EditedAt = message.EditedAt,
            IsDeleted = message.IsDeleted,
            EditCount = editCount,
            ReplyCount = replyCount,
        };
    }

    // ========== HELPER METHODS ==========
    private async Task<ChatMessage> GetMessageWithAllIncludesAsync(Guid messageId)
    {
        return await _context.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.ParentMessage)
                .ThenInclude(pm => pm!.Sender)
            .Include(m => m.Reactions)
                .ThenInclude(r => r.User)
            .Include(m => m.EditHistory)
            .Include(m => m.Replies)
            .FirstAsync(m => m.Id == messageId);
    }


    public ChatMessageDto MapToDto(ChatMessage message, Guid currentUserId)
    {
        var reactionCounts = message.Reactions?.GroupBy(r => r.ReactionType)
            .ToDictionary(g => g.Key, g => g.Count()) ?? new Dictionary<ReactionType, int>();

        var userReactions = message.Reactions?.Where(r => r.UserId == currentUserId)
            .Select(r => r.ReactionType).ToList() ?? new List<ReactionType>();

        return new ChatMessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = message.Sender != null
                ? $"{message.Sender.FirstName} {message.Sender.LastName}"
                : "Unknown User",
            SenderAvatar = message.Sender?.AvatarUrl,
            RecipientId = message.RecipientId,
            GroupId = message.GroupId,
            Content = message.Content,
            MessageType = message.MessageType,
            Status = message.Status,
            SentAt = message.SentAt,
            DeliveredAt = message.DeliveredAt,
            ReadAt = message.ReadAt,
            ParentMessageId = message.ParentMessageId,
            AttachmentUrl = message.AttachmentUrl,
            AttachmentType = message.AttachmentType,
            IsOwnMessage = message.SenderId == currentUserId,

            // Enhanced features
            EditedAt = message.EditedAt,
            IsEdited = message.EditedAt.HasValue,
            IsDeleted = message.IsDeleted,
            EditCount = message.EditHistory?.Count ?? 0,
            ReplyCount = message.Replies?.Count ?? 0,
            Reactions = reactionCounts.Any() || userReactions.Any() 
                ? new MessageReactionsSummaryDto(message.Id, reactionCounts, userReactions)
                : null
        };
    }

}