using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Commons;

namespace OrganizationServices;

public class ApplicationService(IApplicationRepository applicationRepository, ISchoolService schoolService): IApplicationService
{
    public async Task<ApplicationResponseDto?> GetApplicationByIdAsync(Guid id)
    {
        var application = await applicationRepository.GetApplicationByIdAsync(id);

        return application == null ? null : ApplicationResponseDto.FromEntity(application);
    }

    public async Task<List<ApplicationResponseDto>> GetAllApplicationsAsync()
    {
        var applications = await applicationRepository.GetAllApplicationsAsync();
        
       return applications.Select(ApplicationResponseDto.FromEntity).ToList();
    }

    public async Task<ApplicationResponseDto> CreateApplicationAsync(CreateSchoolApplicationDto application)
    {
        var schoolId = await schoolService.CreateSchoolAsync(application.SchoolName, application.Address);
        var applicationEntity = await applicationRepository.CreateApplicationAsync(CreateSchoolApplicationDto.ToEntity(application, schoolId));
        return ApplicationResponseDto.FromEntity(applicationEntity);
    }

    public async Task<ApplicationResponseDto?> UpdateApplicationAsync(Guid id,
        UpdateSchoolApplicationDto updatedApplication)
    {
        var application =
            await applicationRepository.UpdateApplicationAsync(id,
                UpdateSchoolApplicationDto.ToEntity(updatedApplication));
        
        return application != null ? ApplicationResponseDto.FromEntity(application) : null;
        
    }

    public async Task<bool> DeleteApplicationAsync(Guid id) =>
        await applicationRepository.DeleteApplicationAsync(id);
}