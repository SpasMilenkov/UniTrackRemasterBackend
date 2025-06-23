namespace UniTrackRemaster.Api.Dto.Chat;

public record GetMessageRepliesResponseDto(
    List<ChatMessageDto> Replies,
    int TotalCount,
    bool HasMore
);