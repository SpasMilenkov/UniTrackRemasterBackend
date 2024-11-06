using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Commons;

namespace OrganizationServices;

public class ApplicationService(IApplicationRepository applicationRepository, ISchoolService schoolService): IApplicationService
{
    public async Task<ApplicationResponseDto?> GetApplicationByIdAsync(Guid id)
    {
        var application = await applicationRepository.GetApplicationByIdAsync(id);

        return application == null ? null : ApplicationResponseDto.FromEntity(application, SchoolAddressResponseDto
            .FromEntity(application.School.Address, application.School.Id));;
    }

    public async Task<ApplicationResponseDto?> GetApplicationByCodeAsync(string code, string email)
    {
        var application = await applicationRepository.GetApplicationByEmailAsync(email);
        if (application == null) throw new InvalidOperationException();
        if(application.Code != code) throw new InvalidOperationException();

        return ApplicationResponseDto.FromEntity(application, SchoolAddressResponseDto
            .FromEntity(application.School.Address, application.School.Id));;
    }

    public async Task<ApplicationResponseDto?> GetApplicationBySchoolIdAsync(Guid id)
    {
        var application = await applicationRepository.GetApplicationBySchoolIdAsync(id);
        return application == null ? null : ApplicationResponseDto.FromEntity(application, SchoolAddressResponseDto
            .FromEntity(application.School.Address, application.School.Id));;
    }
    
    public async Task<List<ApplicationResponseDto>> GetAllApplicationsAsync()
    {
        var applications = await applicationRepository.GetAllApplicationsAsync();
        
       return applications
           .Select(a => ApplicationResponseDto
               .FromEntity(a, SchoolAddressResponseDto
                   .FromEntity(a.School.Address, a.School.Id))).ToList();
    }

    public async Task ApproveApplicationAsync(Guid id)
    {
        await applicationRepository.ApproveApplication(id);
    }
    public async Task<ApplicationResponseDto?> GetApplicationByEmailAsync(string email)
    {
        var application = await applicationRepository.GetApplicationByEmailAsync(email);
        return application == null ? null : ApplicationResponseDto.FromEntity(application, SchoolAddressResponseDto
            .FromEntity(application.School.Address, application.School.Id));
    }

    public async Task<ApplicationResponseDto> CreateApplicationAsync(CreateSchoolApplicationDto application)
    {
        //Check if a school with such email is not registered already
        //Probably is not ideal to rely on that only but for now shall do
        var exisingApplication = await GetApplicationByEmailAsync(application.Email);
        if (exisingApplication is not null) throw new InvalidOperationException();
        var schoolId = await schoolService.CreateSchoolAsync(application.SchoolName, application.Address);
        
        var applicationEntity = await applicationRepository
            .CreateApplicationAsync(CreateSchoolApplicationDto.ToEntity(application, schoolId));
        return ApplicationResponseDto.FromEntity(applicationEntity, SchoolAddressResponseDto
            .FromEntity(applicationEntity.School.Address, applicationEntity.School.Id));
    }

    public async Task<ApplicationResponseDto?> UpdateApplicationAsync(Guid id,
        UpdateSchoolApplicationDto updatedApplication)
    {
        var application =
            await applicationRepository.UpdateApplicationAsync(id,
                UpdateSchoolApplicationDto.ToEntity(updatedApplication));
        
        return application != null ? ApplicationResponseDto.FromEntity(application, SchoolAddressResponseDto
            .FromEntity(application.School.Address, application.School.Id)) : null;
        
    }

    public async Task<bool> DeleteApplicationAsync(Guid id) =>
        await applicationRepository.DeleteApplicationAsync(id);
}