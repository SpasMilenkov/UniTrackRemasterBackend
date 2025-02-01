using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Commons;

public interface ISemesterRepository
{
    Task<Semester?> GetByIdAsync(Guid id);
    Task<IEnumerable<Semester>> GetByAcademicYearAsync(Guid academicYearId);
    Task<Semester> CreateAsync(Semester semester);
    Task UpdateAsync(Semester semester);
    Task DeleteAsync(Guid id);
}