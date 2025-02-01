using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Commons;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(Guid id);
    Task<IEnumerable<Course>> GetBySemesterAsync(Guid semesterId);
    Task<IEnumerable<Course>> GetBySubjectAsync(Guid subjectId);
    Task<IEnumerable<Course>> GetByMajorAsync(Guid majorId);
    Task<Course> CreateAsync(Course course);
    Task UpdateAsync(Course course);
    Task DeleteAsync(Guid id);
}
