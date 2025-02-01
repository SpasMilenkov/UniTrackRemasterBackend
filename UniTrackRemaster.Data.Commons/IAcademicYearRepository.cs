using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Commons;

public interface IAcademicYearRepository
{
    Task<AcademicYear?> GetByIdAsync(Guid id);
    Task<IEnumerable<AcademicYear>> GetByInstitutionAsync(Guid institutionId);
    Task<AcademicYear> CreateAsync(AcademicYear academicYear);
    Task UpdateAsync(AcademicYear academicYear);
    Task DeleteAsync(Guid id);
}
