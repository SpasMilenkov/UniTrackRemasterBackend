using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Data.Models.Location;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Commons;

public interface ISchoolRepository
{
    
    Task<School> InitSchoolAsync(InitSchoolDto initDto);
    Task<School> GetSchoolAsync(Guid schoolId);
    Task<List<School>> GetSchoolsAsync(SchoolFilterDto filter);
    Task<School> UpdateSchoolAsync(UpdateSchoolDto updateDto);
    Task DeleteSchoolAsync(Guid schoolId);
}