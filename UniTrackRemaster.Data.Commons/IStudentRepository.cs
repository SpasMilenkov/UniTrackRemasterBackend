using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Commons;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id);
    Task<Student?> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Student>> GetBySchoolAsync(Guid schoolId);
    Task<IEnumerable<Student>> GetByUniversityAsync(Guid universityId);
    Task<IEnumerable<Student>> GetByGradeAsync(Guid gradeId);
    Task<Student> CreateAsync(Student student);
    Task UpdateAsync(Student student);
    Task DeleteAsync(Guid id);
}
