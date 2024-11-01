using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Data.Models.Location;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Commons;

public interface ISchoolRepository
{
    Task<School> CreateSchoolAsync(string name, SchoolAddress address);
    Task<School> InitSchoolAsync(InitSchoolDto initDto);
    Task<School> GetSchoolAsync(Guid schoolId);
    Task<List<School>> GetSchoolsAsync(int pageNumber, int pageSize);
    Task<School> UpdateSchoolAsync(UpdateSchoolDto updateDto);
    Task DeleteSchoolAsync(Guid schoolId);
}