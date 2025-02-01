using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Commons;

public interface IAttendanceRepository
{
    Task<Attendance?> GetByIdAsync(Guid id);
    Task<IEnumerable<Attendance>> GetByStudentAsync(Guid studentId);
    Task<IEnumerable<Attendance>> GetByCourseAsync(Guid courseId);
    Task<IEnumerable<Attendance>> GetBySubjectAsync(Guid subjectId);
    Task<Attendance> CreateAsync(Attendance attendance);
    Task UpdateAsync(Attendance attendance);
    Task DeleteAsync(Guid id);
}
