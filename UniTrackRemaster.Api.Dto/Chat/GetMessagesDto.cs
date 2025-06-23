namespace UniTrackRemaster.Api.Dto.Chat;

public record GetMessagesDto(
    string ConversationType,
    Guid? OtherUserId = null,
    Guid? GroupId = null,
    int Page = 1,
    int PageSize = 50,
    DateTime? Before = null
);