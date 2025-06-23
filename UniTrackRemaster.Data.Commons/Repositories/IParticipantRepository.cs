using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Commons.Repositories;

public interface IParticipantRepository
{
    Task<Participant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Participant>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Participant>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Participant?> GetByEventAndUserAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
    Task<Participant> CreateAsync(Participant participant, CancellationToken cancellationToken = default);
    Task<Participant> UpdateAsync(Participant participant, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteByEventAndUserAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsUserParticipantAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
    Task<Dictionary<ParticipantStatus, int>> GetParticipantCountsByStatusAsync(Guid eventId, CancellationToken cancellationToken = default);
}