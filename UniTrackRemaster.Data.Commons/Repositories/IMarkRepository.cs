using UniTrackRemaster.Api.Dto.Mark;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Commons.Repositories;

/// <summary>
/// Interface for mark repository operations with semester support
/// </summary>
public interface IMarkRepository : IRepository<Mark>
{
    Task<Mark?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Mark>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Mark> AddAsync(Mark mark, CancellationToken cancellationToken = default);
    Task UpdateAsync(Mark mark, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Semester-aware methods
    Task<IEnumerable<Mark>> GetMarksByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Mark>> GetMarksByStudentIdAsync(Guid studentId, Guid? semesterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Mark>> GetMarksByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Mark>> GetMarksByTeacherIdAsync(Guid teacherId, Guid? semesterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Mark>> GetMarksBySubjectIdAsync(Guid subjectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Mark>> GetMarksBySubjectIdAsync(Guid subjectId, Guid? semesterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Mark>> GetMarksByFilterAsync(MarkFilterParams filterParams, CancellationToken cancellationToken = default);
    
    // Statistics methods
    Task<decimal> GetAverageMarkForStudentAsync(Guid studentId, Guid? subjectId = null, CancellationToken cancellationToken = default);
    Task<decimal> GetAverageMarkForStudentAsync(Guid studentId, Guid? subjectId = null, Guid? semesterId = null, CancellationToken cancellationToken = default);
    Task<decimal> GetAverageMarkForSubjectAsync(Guid subjectId, CancellationToken cancellationToken = default);
    Task<decimal> GetAverageMarkForSubjectAsync(Guid subjectId, Guid? semesterId = null, CancellationToken cancellationToken = default);
}