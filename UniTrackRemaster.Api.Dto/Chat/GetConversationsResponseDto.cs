namespace UniTrackRemaster.Api.Dto.Chat;

public record GetConversationsResponseDto(
    List<ConversationDto> Conversations,
    int TotalCount
);