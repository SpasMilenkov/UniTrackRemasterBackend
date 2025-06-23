namespace UniTrackRemaster.Api.Dto.Chat;

public record JoinConversationDto(
    string ConversationType, // "direct" or "group"
    string GroupType = "institution",
    Guid? OtherUserId = null, // For direct conversations
    Guid? GroupId = null // For group conversations
);
