using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Commons.Repositories;

public interface IEventNotificationRepository
{
    Task<EventNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventNotification>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventNotification>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventNotification>> GetPendingNotificationsAsync(CancellationToken cancellationToken = default);
    Task<EventNotification> CreateAsync(EventNotification notification, CancellationToken cancellationToken = default);
    Task<EventNotification> UpdateAsync(EventNotification notification, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAsSentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventNotification>> CreateBulkAsync(IEnumerable<EventNotification> notifications, CancellationToken cancellationToken = default);
}