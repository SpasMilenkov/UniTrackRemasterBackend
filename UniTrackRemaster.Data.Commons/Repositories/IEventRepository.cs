using UniTrackRemaster.Api.Dto.Event;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Commons.Repositories;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, bool includeRelated = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> GetByOrganizerAsync(Guid organizerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> GetByInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> GetByParticipantAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> GetUpcomingEventsAsync(Guid? userId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> GetFilteredAsync(EventFilterParams filterParams, CancellationToken cancellationToken = default);
    Task<Event> CreateAsync(Event eventEntity, CancellationToken cancellationToken = default);
    Task<Event> UpdateAsync(Event eventEntity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<EventStatus, int>> GetEventCountsByStatusAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<EventType, int>> GetEventCountsByTypeAsync(CancellationToken cancellationToken = default);
}