using System;

namespace UniTrackRemaster.Api.Dto.Chat.Events;

public class ChatMessageDeletedEvent
{
    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; }
    public Guid? RecipientId { get; set; }
    public Guid? GroupId { get; set; }
    public Guid DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
    public bool IsHardDelete { get; set; }
}
