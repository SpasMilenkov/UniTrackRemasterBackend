using UniTrackRemaster.Api.Dto.Event;

namespace UniTrackRemaster.Commons.Services;

public interface IEventService
{
    Task<EventResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EventDetailResponseDto> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<EventResponseDto>> GetByOrganizerAsync(Guid organizerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventResponseDto>> GetByInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventResponseDto>> GetByParticipantAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventResponseDto>> GetUpcomingEventsAsync(Guid? userId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventResponseDto>> GetFilteredAsync(EventFilterDto filterDto, CancellationToken cancellationToken = default);
    Task<EventResponseDto> CreateAsync(CreateEventDto createDto, Guid organizerUserId, CancellationToken cancellationToken = default);
    Task<EventResponseDto> UpdateAsync(Guid id, UpdateEventDto updateDto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EventStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
    Task<bool> CanUserModifyEventAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
}
