using System;
using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Api.Dto.Event;

public record CreateEventNotificationDto(
    [Required] Guid EventId,
    [Required] Guid UserId,
    [Required] NotificationType Type,
    [Required] DateTime SendAt,
    string? Message)
{
    public EventNotification ToEntity() => new()
    {
        Id = Guid.NewGuid(),
        EventId = EventId,
        UserId = UserId,
        Type = Type,
        SendAt = SendAt,
        IsSent = false,
        Message = Message,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
