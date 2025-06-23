using System;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Events;

public class EventNotification : BaseEntity
{
    public Guid Id { get; set; }
    public required Guid EventId { get; set; }
    public Event? Event { get; set; }

    public required Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public NotificationType Type { get; set; }
    public DateTime SendAt { get; set; }
    public bool IsSent { get; set; }
    public string? Message { get; set; }
}
