namespace UniTrackRemaster.Api.Dto.Chat;

public record SendDirectMessageDto(
    Guid RecipientId,
    string Content,
    Guid? ParentMessageId = null
);