namespace UniTrackRemaster.Api.Dto.Chat;

public record SendGroupMessageDto(
    Guid GroupId,
    string GroupType, // "institution" or "class"
    string Content,
    Guid? ParentMessageId = null
);