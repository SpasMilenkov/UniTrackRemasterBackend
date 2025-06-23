namespace UniTrackRemaster.Api.Dto.Chat;

public record GetMessagesResponseDto(
    List<ChatMessageDto> Messages,
    int TotalCount,
    bool HasMore
);