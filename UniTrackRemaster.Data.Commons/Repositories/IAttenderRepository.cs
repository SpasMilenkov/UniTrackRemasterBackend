using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Commons.Repositories;

public interface IAttenderRepository
{
    Task<Attender?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Attender>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Attender>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Attender?> GetByEventAndUserAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
    Task<Attender> CreateAsync(Attender attender, CancellationToken cancellationToken = default);
    Task<Attender> UpdateAsync(Attender attender, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Dictionary<AttendanceStatus, int>> GetAttendanceCountsByStatusAsync(Guid eventId, CancellationToken cancellationToken = default);
}
