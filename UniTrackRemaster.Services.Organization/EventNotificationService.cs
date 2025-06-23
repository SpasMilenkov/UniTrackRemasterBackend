using System;
using UniTrackRemaster.Api.Dto.Event;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Services.Organization;

public class EventNotificationService : IEventNotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public EventNotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<EventNotificationResponseDto>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var notifications = await _unitOfWork.EventNotifications.GetByEventAsync(eventId, cancellationToken);
        return notifications.Select(EventNotificationResponseDto.FromEntity);
    }

    public async Task<IEnumerable<EventNotificationResponseDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _unitOfWork.EventNotifications.GetByUserAsync(userId, cancellationToken);
        return notifications.Select(EventNotificationResponseDto.FromEntity);
    }

    public async Task<EventNotificationResponseDto> CreateAsync(CreateEventNotificationDto createDto, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var notification = createDto.ToEntity();
            var createdNotification = await _unitOfWork.EventNotifications.CreateAsync(notification, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return EventNotificationResponseDto.FromEntity(createdNotification);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ScheduleEventNotificationsAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId, true, cancellationToken);
            if (eventEntity == null) return;

            var participants = await _unitOfWork.Participants.GetByEventAsync(eventId, cancellationToken);
            var notifications = new List<EventNotification>();

            foreach (var participant in participants)
            {
                // Initial invitation
                notifications.Add(new EventNotification
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    UserId = participant.UserId,
                    Type = NotificationType.Invitation,
                    SendAt = DateTime.UtcNow,
                    IsSent = false,
                    Message = $"You have been invited to {eventEntity.Title}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                // Reminder 24 hours before
                if (eventEntity.StartDate > DateTime.UtcNow.AddDays(1))
                {
                    notifications.Add(new EventNotification
                    {
                        Id = Guid.NewGuid(),
                        EventId = eventId,
                        UserId = participant.UserId,
                        Type = NotificationType.Reminder,
                        SendAt = eventEntity.StartDate.AddDays(-1),
                        IsSent = false,
                        Message = $"Reminder: {eventEntity.Title} is tomorrow",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                // Starting soon notification
                if (eventEntity.StartDate > DateTime.UtcNow.AddMinutes(30))
                {
                    notifications.Add(new EventNotification
                    {
                        Id = Guid.NewGuid(),
                        EventId = eventId,
                        UserId = participant.UserId,
                        Type = NotificationType.StartingSoon,
                        SendAt = eventEntity.StartDate.AddMinutes(-30),
                        IsSent = false,
                        Message = $"{eventEntity.Title} is starting in 30 minutes",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            await _unitOfWork.EventNotifications.CreateBulkAsync(notifications, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task SendEventInvitationAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId, false, cancellationToken);
            if (eventEntity == null) return;

            var notification = new EventNotification
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                Type = NotificationType.Invitation,
                SendAt = DateTime.UtcNow,
                IsSent = false,
                Message = $"You have been invited to {eventEntity.Title}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.EventNotifications.CreateAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task NotifyEventUpdateAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId, false, cancellationToken);
            if (eventEntity == null) return;

            var participants = await _unitOfWork.Participants.GetByEventAsync(eventId, cancellationToken);
            var notifications = new List<EventNotification>();

            foreach (var participant in participants)
            {
                notifications.Add(new EventNotification
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    UserId = participant.UserId,
                    Type = NotificationType.Update,
                    SendAt = DateTime.UtcNow,
                    IsSent = false,
                    Message = $"Event '{eventEntity.Title}' has been updated",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.EventNotifications.CreateBulkAsync(notifications, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task NotifyEventCancellationAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId, false, cancellationToken);
            if (eventEntity == null) return;

            var participants = await _unitOfWork.Participants.GetByEventAsync(eventId, cancellationToken);
            var notifications = new List<EventNotification>();

            foreach (var participant in participants)
            {
                notifications.Add(new EventNotification
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    UserId = participant.UserId,
                    Type = NotificationType.Cancellation,
                    SendAt = DateTime.UtcNow,
                    IsSent = false,
                    Message = $"Event '{eventEntity.Title}' has been cancelled",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.EventNotifications.CreateBulkAsync(notifications, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ProcessPendingNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var pendingNotifications = await _unitOfWork.EventNotifications.GetPendingNotificationsAsync(cancellationToken);

        foreach (var notification in pendingNotifications)
        {
            try
            {
                // Here you would integrate with your notification system (email, push notifications, etc.)
                // For now, we'll just mark them as sent
                await _unitOfWork.EventNotifications.MarkAsSentAsync(notification.Id, cancellationToken);
            }
            catch (Exception)
            {
                // Log error but continue processing other notifications
            }
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _unitOfWork.EventNotifications.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
