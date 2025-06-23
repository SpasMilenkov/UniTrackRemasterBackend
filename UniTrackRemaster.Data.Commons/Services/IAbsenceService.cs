using UniTrackRemaster.Api.Dto.Absence;
using UniTrackRemaster.Api.Dto.Teacher.Analytics;

namespace UniTrackRemaster.Commons.Services;

/// <summary>
/// Updated interface for AbsenceService with semester support
/// </summary>
public interface IAbsenceService
{
    Task<IEnumerable<AbsenceResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AbsenceResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AbsenceResponseDto>> GetByStudentIdAsync(Guid studentId, Guid? semesterId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<AbsenceResponseDto>> GetAbsencesByTeacherAsync(Guid teacherId, Guid? semesterId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<AbsenceResponseDto>> GetByStudentAndSubjectAsync(Guid studentId, Guid subjectId, Guid? semesterId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<AbsenceResponseDto>> GetBySubjectAsync(Guid subjectId, Guid? semesterId = null, CancellationToken cancellationToken = default);
    Task<AbsenceResponseDto> CreateAsync(CreateAbsenceDto createAbsenceDto, CancellationToken cancellationToken = default);
    Task<AbsenceResponseDto> UpdateAsync(Guid id, UpdateAbsenceDto updateAbsenceDto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StudentAttendanceStatsDto> GetStudentAttendanceStatsAsync(Guid studentId, Guid? semesterId = null, CancellationToken cancellationToken = default);
    Task<SubjectAttendanceSummaryDto> GetSubjectAttendanceSummaryAsync(Guid subjectId, Guid? semesterId = null, CancellationToken cancellationToken = default);
}