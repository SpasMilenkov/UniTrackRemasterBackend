using System;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Chat.Events;

public class MessageReactionAddedEvent
{
    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; } // Message sender
    public Guid? RecipientId { get; set; }
    public Guid? GroupId { get; set; }
    public Guid ReactedBy { get; set; } // Who added the reaction
    public string ReactedByName { get; set; } = string.Empty;
    public ReactionType ReactionType { get; set; }
    public DateTime ReactedAt { get; set; }
    public Dictionary<ReactionType, int> UpdatedReactionCounts { get; set; } = new();
}