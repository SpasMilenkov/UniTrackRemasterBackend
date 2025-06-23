using System;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Api.Dto.Event;

public record EventNotificationResponseDto(
     Guid Id,
     Guid EventId,
     Guid UserId,
     string UserName,
     NotificationType Type,
     DateTime SendAt,
     bool IsSent,
     string? Message,
     DateTime CreatedAt)
{
    public static EventNotificationResponseDto FromEntity(EventNotification notification) => new(
        notification.Id,
        notification.EventId,
        notification.UserId,
        notification.User != null ? $"{notification.User.FirstName} {notification.User.LastName}" : "",
        notification.Type,
        notification.SendAt,
        notification.IsSent,
        notification.Message,
        notification.CreatedAt
    );
}
