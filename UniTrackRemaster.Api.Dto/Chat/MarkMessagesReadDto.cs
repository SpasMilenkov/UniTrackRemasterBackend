namespace UniTrackRemaster.Api.Dto.Chat;

public record MarkMessagesReadDto(
    List<Guid> MessageIds
);