using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Chat;

public record MessageReactionsSummaryDto(
    Guid MessageId,
    Dictionary<ReactionType, int> ReactionCounts,
    List<ReactionType> UserReactions
);
