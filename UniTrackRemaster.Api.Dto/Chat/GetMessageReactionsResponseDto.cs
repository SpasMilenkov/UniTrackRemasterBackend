using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Chat;


public record GetMessageReactionsResponseDto(
    List<ReactionDto> Reactions,
    Dictionary<ReactionType, int> ReactionCounts
);