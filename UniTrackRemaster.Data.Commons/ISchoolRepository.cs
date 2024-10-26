using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Commons;

public interface ISchoolRepository
{
    Task<School> CreateSchoolAsync(CreateSchoolDto createDto);
    Task<School> GetSchoolAsync(Guid schoolId);
    Task<School> UpdateSchoolAsync(UpdateSchoolDto updateDto);
    Task DeleteSchoolAsync(Guid schoolId);
}