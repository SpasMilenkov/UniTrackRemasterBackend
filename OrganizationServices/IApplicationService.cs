using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;

namespace OrganizationServices;

public interface IApplicationService
{
    public Task<ApplicationResponseDto?> GetApplicationByIdAsync(Guid id);
    public Task<List<ApplicationResponseDto>> GetAllApplicationsAsync();
    public Task<ApplicationResponseDto> CreateApplicationAsync(CreateSchoolApplicationDto application);
    public Task<ApplicationResponseDto?> UpdateApplicationAsync(Guid id,
        CreateSchoolApplicationDto updatedApplication);
    public Task<bool> DeleteApplicationAsync(Guid id);

}