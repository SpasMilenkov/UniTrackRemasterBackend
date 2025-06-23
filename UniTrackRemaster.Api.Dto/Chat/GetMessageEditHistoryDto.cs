namespace UniTrackRemaster.Api.Dto.Chat;

public record GetMessageEditHistoryResponseDto(
    List<MessageEditHistoryDto> EditHistory,
    int TotalCount
);
