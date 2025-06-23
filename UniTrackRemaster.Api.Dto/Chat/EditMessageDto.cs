namespace UniTrackRemaster.Api.Dto.Chat;

public record EditMessageDto(
    string Content,
    string? EditReason = null
);