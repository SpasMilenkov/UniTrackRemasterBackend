using UniTrackRemaster.Api.Dto.Event;

namespace UniTrackRemaster.Commons.Services;

public interface IOrganizerService
{
    Task<OrganizerResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrganizerResponseDto> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrganizerResponseDto>> GetByInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default);
    Task<OrganizerResponseDto> CreateAsync(CreateOrganizerDto createDto, CancellationToken cancellationToken = default);
    Task<OrganizerResponseDto> UpdateAsync(Guid id, CreateOrganizerDto updateDto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsUserOrganizerAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CanCreatePublicEventsAsync(Guid userId, CancellationToken cancellationToken = default);
}
