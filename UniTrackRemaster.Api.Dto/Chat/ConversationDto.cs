namespace UniTrackRemaster.Api.Dto.Chat;

public record ConversationDto(
    Guid Id,
    string Type, // "direct" or "group"
    string Name,
    string? Avatar,
    Guid? OtherUserId, // For direct conversations
    Guid? GroupId, // For group conversations
    ChatMessageDto? LastMessage,
    int UnreadCount,
    DateTime? LastActivity,
    List<Guid> OnlineUserIds
);