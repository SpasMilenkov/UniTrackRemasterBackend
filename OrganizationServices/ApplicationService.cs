using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Commons;

namespace OrganizationServices;

public class ApplicationService(IApplicationRepository applicationRepository): IApplicationService
{
    public async Task<ApplicationResponseDto?> GetApplicationByIdAsync(Guid id) =>
        await applicationRepository.GetApplicationByIdAsync(id);

    public async Task<List<ApplicationResponseDto>> GetAllApplicationsAsync() =>
        await applicationRepository.GetAllApplicationsAsync();

    public async Task<ApplicationResponseDto> CreateApplicationAsync(CreateSchoolApplicationDto application)
    {
        const school = await schoolService.CreateSchoolAsync(application.SchoolName);
        await applicationRepository.CreateApplicationAsync(application);
    }

    public async Task<ApplicationResponseDto?> UpdateApplicationAsync(Guid id, CreateSchoolApplicationDto updatedApplication) =>
        await applicationRepository.UpdateApplicationAsync(id, updatedApplication);

    public async Task<bool> DeleteApplicationAsync(Guid id) =>
        await applicationRepository.DeleteApplicationAsync(id);
}