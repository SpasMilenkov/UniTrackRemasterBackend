namespace UniTrackRemaster.Api.Dto.Chat;

public record SearchMessagesDto(
    string SearchTerm,
    string? ConversationType = null,
    Guid? ConversationId = null
);