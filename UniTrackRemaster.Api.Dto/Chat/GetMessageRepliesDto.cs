namespace UniTrackRemaster.Api.Dto.Chat;

public record GetMessageRepliesDto(
    Guid ParentMessageId,
    int Page = 1,
    int PageSize = 20
);