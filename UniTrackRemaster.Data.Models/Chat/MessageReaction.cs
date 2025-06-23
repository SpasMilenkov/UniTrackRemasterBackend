using System;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Chat;

public class MessageReaction : BaseEntity
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public ChatMessage Message { get; set; } = null!;

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public ReactionType ReactionType { get; set; }
    public DateTime ReactedAt { get; set; }
}
