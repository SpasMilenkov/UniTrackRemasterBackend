using UniTrackRemaster.Api.Dto.Event;

namespace UniTrackRemaster.Commons.Services;

public interface IEventNotificationService
{
    Task<IEnumerable<EventNotificationResponseDto>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventNotificationResponseDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<EventNotificationResponseDto> CreateAsync(CreateEventNotificationDto createDto, CancellationToken cancellationToken = default);
    Task ScheduleEventNotificationsAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task SendEventInvitationAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
    Task NotifyEventUpdateAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task NotifyEventCancellationAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task ProcessPendingNotificationsAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}