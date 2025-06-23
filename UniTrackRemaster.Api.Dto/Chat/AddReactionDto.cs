using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Chat;

public record AddReactionDto(
    ReactionType ReactionType
);