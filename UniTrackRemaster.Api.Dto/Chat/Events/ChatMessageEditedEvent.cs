using System;

namespace UniTrackRemaster.Api.Dto.Chat.Events;

public class ChatMessageEditedEvent
{
    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; }
    public Guid? RecipientId { get; set; }
    public Guid? GroupId { get; set; }
    public string NewContent { get; set; } = string.Empty;
    public DateTime EditedAt { get; set; }
    public string? EditReason { get; set; }
    public int EditCount { get; set; }
}
