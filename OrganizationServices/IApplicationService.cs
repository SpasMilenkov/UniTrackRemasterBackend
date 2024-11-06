using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;

namespace OrganizationServices;

public interface IApplicationService
{
    public Task<ApplicationResponseDto?> GetApplicationByIdAsync(Guid id);
    public Task<List<ApplicationResponseDto>> GetAllApplicationsAsync();
    public Task<ApplicationResponseDto?> GetApplicationByCodeAsync(string code, string email);
    public Task<ApplicationResponseDto?> GetApplicationBySchoolIdAsync(Guid id);
    public Task<ApplicationResponseDto> CreateApplicationAsync(CreateSchoolApplicationDto application);
    public Task<ApplicationResponseDto?> UpdateApplicationAsync(Guid id,
        UpdateSchoolApplicationDto updatedApplication);
    public Task ApproveApplicationAsync(Guid id);
    public Task<bool> DeleteApplicationAsync(Guid id);

}