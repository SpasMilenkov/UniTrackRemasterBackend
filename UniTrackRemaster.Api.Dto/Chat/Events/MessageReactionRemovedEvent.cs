using System;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Chat.Events;

public class MessageReactionRemovedEvent
{
    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; } // Message sender
    public Guid? RecipientId { get; set; }
    public Guid? GroupId { get; set; }
    public Guid RemovedBy { get; set; } // Who removed the reaction
    public ReactionType ReactionType { get; set; }
    public DateTime RemovedAt { get; set; }
    public Dictionary<ReactionType, int> UpdatedReactionCounts { get; set; } = new();
}