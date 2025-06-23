using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Chat;

public record ReactionDto(
    Guid Id,
    Guid UserId,
    string UserName,
    ReactionType ReactionType,
    DateTime ReactedAt
);