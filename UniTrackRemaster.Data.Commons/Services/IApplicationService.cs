using UniTrackRemaster.Api.Dto.Application;

namespace UniTrackRemaster.Commons.Services;

public interface IApplicationService
{
    Task<ApplicationResponseDto> GetByIdAsync(Guid id);
    Task<ApplicationResponseDto> GetByCodeAsync(string code, string email);
    Task<ApplicationResponseDto> GetByInstitutionIdAsync(Guid id);
    Task<PagedResult<ApplicationResponseDto>> GetAllAsync(string? statusFilter = null, int page = 1, int pageSize = 50);
    Task ApproveAsync(Guid id);
    Task<ApplicationResponseDto> CreateAsync(CreateInstitutionApplicationDto application);
    Task<ApplicationResponseDto> UpdateAsync(Guid id, UpdateInstitutionApplicationDto updatedApplication);
    Task<bool> DeleteAsync(Guid id);
}