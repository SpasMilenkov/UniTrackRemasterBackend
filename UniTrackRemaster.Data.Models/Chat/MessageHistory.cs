using System;

namespace UniTrackRemaster.Data.Models.Chat;

public class MessageEditHistory : BaseEntity
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public ChatMessage Message { get; set; } = null!;

    public string PreviousContent { get; set; } = string.Empty;
    public string NewContent { get; set; } = string.Empty;
    public DateTime EditedAt { get; set; }
    public string? EditReason { get; set; } // Optional: why was it edited
}