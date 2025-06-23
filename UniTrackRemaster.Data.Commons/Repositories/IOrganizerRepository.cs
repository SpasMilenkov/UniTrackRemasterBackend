using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Commons.Repositories;

public interface IOrganizerRepository
{
    Task<Organizer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Organizer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Organizer>> GetByInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Organizer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Organizer> CreateAsync(Organizer organizer, CancellationToken cancellationToken = default);
    Task<Organizer> UpdateAsync(Organizer organizer, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsUserOrganizerAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CanCreatePublicEventsAsync(Guid userId, CancellationToken cancellationToken = default);
}
