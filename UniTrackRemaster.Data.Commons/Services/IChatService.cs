using UniTrackRemaster.Api.Dto.Chat;
using UniTrackRemaster.Data.Models.Chat;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Commons.Services;

public interface IChatService
{
    // ========== EXISTING METHODS ==========
    Task<GetConversationsResponseDto> GetConversationsAsync(Guid userId);
    Task<ConversationDto?> GetConversationAsync(Guid userId, string conversationType, Guid? otherUserId = null, Guid? groupId = null);
    Task<GetMessagesResponseDto> GetMessagesAsync(Guid userId, GetMessagesDto dto);
    Task<ChatMessageDto> SendDirectMessageAsync(Guid senderId, SendDirectMessageDto dto);
    Task<ChatMessageDto> SendGroupMessageAsync(Guid senderId, SendGroupMessageDto dto);
    Task<List<ChatMessageDto>> SearchMessagesAsync(Guid userId, SearchMessagesDto dto);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkMessagesAsReadAsync(Guid userId, List<Guid> messageIds);
    Task<List<Guid>> GetMessageRecipientsAsync(List<Guid> messageIds);
    Task UpdateMessageStatusAsync(Guid messageId, MessageStatus status);
    Task<bool> UserCanAccessMessageAsync(Guid userId, Guid messageId);
    Task<ChatMessage?> GetMessageByIdAsync(Guid messageId);
    ChatMessageDto MapToDto(ChatMessage message, Guid currentUserId);
    Task<ChatMessageDto> MapToDtoWithReactionsAsync(ChatMessage message, Guid currentUserId);

    /// <summary>
    /// Gets message details needed for real-time events (conversation type, participants, etc.)
    /// </summary>
    Task<dynamic> GetMessageDetailsForEventAsync(Guid messageId);

    /// <summary>
    /// Gets details for multiple messages needed for real-time events
    /// </summary>
    Task<List<dynamic>> GetMessagesDetailsForEventAsync(List<Guid> messageIds);

    // ========== MESSAGE EDITING ==========
    Task<ChatMessageDto> EditMessageAsync(Guid messageId, Guid userId, EditMessageDto dto);
    Task<bool> CanEditMessageAsync(Guid messageId, Guid userId);
    Task<GetMessageEditHistoryResponseDto> GetMessageEditHistoryAsync(GetMessageEditHistoryDto request);

    // ========== MESSAGE DELETION ==========
    Task<bool> DeleteMessageAsync(Guid messageId, Guid userId, DeleteMessageDto dto);
    Task<bool> CanDeleteMessageAsync(Guid messageId, Guid userId);

    // ========== REACTIONS ==========
    Task<MessageReactionsSummaryDto> AddReactionAsync(Guid messageId, Guid userId, AddReactionDto dto);
    Task<MessageReactionsSummaryDto> RemoveReactionAsync(Guid messageId, Guid userId, AddReactionDto dto);
    Task<GetMessageReactionsResponseDto> GetMessageReactionsAsync(GetMessageReactionsDto request);
    Task<Dictionary<ReactionType, int>> GetReactionCountsAsync(Guid messageId);
    

    // ========== REPLIES ==========
    Task<GetMessageRepliesResponseDto> GetMessageRepliesAsync(GetMessageRepliesDto request);

    

    
}
