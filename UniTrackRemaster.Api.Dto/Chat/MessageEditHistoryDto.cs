namespace UniTrackRemaster.Api.Dto.Chat;

public record MessageEditHistoryDto(
    Guid Id,
    string PreviousContent,
    string NewContent,
    DateTime EditedAt,
    string? EditReason
);