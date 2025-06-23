using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Commons.Repositories;

/// <summary>
/// Interface for absence repository operations with semester support
/// </summary>
public interface IAbsenceRepository : IRepository<Absence>
{
    Task<Absence?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Absence>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Absence> AddAsync(Absence absence, CancellationToken cancellationToken = default);
    Task UpdateAsync(Absence absence, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Semester-aware methods
    Task<IEnumerable<Absence>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Absence>> GetByStudentIdAsync(Guid studentId, Guid? semesterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Absence>> GetAbsencesByTeacherAsync(Guid teacherId, Guid? semesterId = null);
    Task<IEnumerable<Absence>> GetByStudentAndSubjectAsync(Guid studentId, Guid subjectId, Guid? semesterId = null);
    Task<IEnumerable<Absence>> GetBySubjectAsync(Guid subjectId, Guid? semesterId = null);
    
    // Analytics methods for teacher service
    Task<IEnumerable<Absence>> GetTeacherAttendanceDataAsync(Guid teacherId, DateTime? fromDate = null, DateTime? toDate = null, Guid? gradeId = null, Guid? subjectId = null, Guid? semesterId = null);
}
