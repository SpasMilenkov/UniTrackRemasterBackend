using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Models.Organizations;

namespace OrganizationServices;

public class SchoolService(ISchoolRepository schoolRepository) : ISchoolService
{

    public async Task<Guid> CreateSchoolAsync(string name, AddressRequestDto address)
    {
        var school = await schoolRepository.CreateSchoolAsync(name, AddressRequestDto.ToEntity(address));
        return school.Id;
    }

    public async Task<Guid> InitSchoolAsync(InitSchoolDto initDto)
    {
        var school = await schoolRepository.InitSchoolAsync(initDto);
        return school.Id;
    }

    public async Task<SchoolResponseDto> GetSchoolAsync(Guid schoolId)
    {
        var school = await schoolRepository.GetSchoolAsync(schoolId);
        
        return SchoolResponseDto.FromEntity(school);
    }

    public async Task<List<SchoolResponseDto>> GetSchoolsAsync(int page = 0, int pageSize = 5)
    {
        var schools = await schoolRepository.GetSchoolsAsync(page, pageSize);
      
        return schools.Select(school => SchoolResponseDto.FromEntity(school)).ToList();
    }

    public async Task<SchoolResponseDto> UpdateSchoolAsync(UpdateSchoolDto updateDto)
    {
        var school =  await schoolRepository.UpdateSchoolAsync(updateDto);
        
        return SchoolResponseDto.FromEntity(school);
    }

    public async Task DeleteSchoolAsync(Guid schoolId)
    {
        await schoolRepository.DeleteSchoolAsync(schoolId);
    }
}