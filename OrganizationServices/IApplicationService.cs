using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Data.Models.Enums;

namespace OrganizationServices;

public interface IApplicationService
{
    Task<ApplicationResponseDto> GetByIdAsync(Guid id);
    Task<List<ApplicationResponseDto>> GetAllAsync();
    Task<ApplicationResponseDto> GetByCodeAsync(string code, string email);
    Task<ApplicationResponseDto> GetByInstitutionIdAsync(Guid id);
    Task<ApplicationResponseDto> CreateAsync(CreateInstitutionApplicationDto application);
    Task<ApplicationResponseDto> UpdateAsync(Guid id, UpdateInstitutionApplicationDto updatedApplication);
    Task ApproveAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}

