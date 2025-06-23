using UniTrackRemaster.Api.Dto.Event;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Commons.Services;

public interface IParticipantService
{
    Task<IEnumerable<ParticipantResponseDto>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ParticipantResponseDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ParticipantResponseDto> AddParticipantAsync(CreateParticipantDto createDto, CancellationToken cancellationToken = default);
    Task<ParticipantResponseDto> UpdateParticipantStatusAsync(Guid eventId, Guid userId, UpdateParticipantStatusDto updateDto, CancellationToken cancellationToken = default);
    Task RemoveParticipantAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsUserParticipantAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
    Task<Dictionary<ParticipantStatus, int>> GetParticipantStatsAsync(Guid eventId, CancellationToken cancellationToken = default);
}