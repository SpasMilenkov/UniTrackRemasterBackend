using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Commons.Repositories;

public interface ISchoolRepository : IRepository<School>
{

    Task<School> InitSchoolAsync(InitSchoolDto initDto);
    Task<School> GetByIdAsync(Guid schoolId);
    Task<School?> GetByInstitutionIdAsync(Guid institutionId);
    Task<List<School>> GetSchoolsAsync(SchoolFilterDto filter);
    Task<School> UpdateSchoolAsync(UpdateSchoolDto updateDto);
    Task DeleteSchoolAsync(Guid schoolId);
}