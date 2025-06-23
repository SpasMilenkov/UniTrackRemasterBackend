namespace UniTrackRemaster.Api.Dto.Chat;

public record TypingIndicatorDto(
    Guid? RecipientId = null,
    Guid? GroupId = null,
    string? GroupType = null
);