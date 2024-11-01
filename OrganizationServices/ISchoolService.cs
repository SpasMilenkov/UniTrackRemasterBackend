using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;

namespace OrganizationServices;

public interface ISchoolService
{
    Task<Guid> CreateSchoolAsync(string name, AddressRequestDto address);
    
    Task<Guid> InitSchoolAsync(InitSchoolDto initDto);
    Task<SchoolResponseDto> GetSchoolAsync(Guid schoolId);
    Task<List<SchoolResponseDto>> GetSchoolsAsync(int page = 0, int pageSize = 5);
    Task<SchoolResponseDto> UpdateSchoolAsync(UpdateSchoolDto updateDto);
    Task DeleteSchoolAsync(Guid schoolId);
}