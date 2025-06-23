using UniTrackRemaster.Api.Dto.Mark;

namespace UniTrackRemaster.Commons.Services;

/// <summary>
/// Updated interface for MarkService with semester support
/// </summary>
public interface IMarkService
{
    Task<IEnumerable<MarkResponseDto>> GetAllMarksAsync(CancellationToken cancellationToken = default);
    Task<MarkResponseDto> GetMarkByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<MarkResponseDto>> GetMarksByStudentIdAsync(Guid studentId, Guid? semesterId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<MarkResponseDto>> GetMarksByTeacherIdAsync(Guid teacherId, Guid? semesterId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<MarkResponseDto>> GetMarksBySubjectIdAsync(Guid subjectId, Guid? semesterId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<MarkResponseDto>> GetMarksByFilterAsync(MarkFilterParams filterParams, CancellationToken cancellationToken = default);
    Task<MarkResponseDto> CreateMarkAsync(CreateMarkDto createMarkDto, CancellationToken cancellationToken = default);
    Task<MarkResponseDto> UpdateMarkAsync(Guid id, UpdateMarkDto updateMarkDto, CancellationToken cancellationToken = default);
    Task DeleteMarkAsync(Guid id, CancellationToken cancellationToken = default);
    Task<decimal> GetAverageMarkForStudentAsync(Guid studentId, Guid? subjectId = null, Guid? semesterId = null, CancellationToken cancellationToken = default);
    Task<decimal> GetAverageMarkForSubjectAsync(Guid subjectId, Guid? semesterId = null, CancellationToken cancellationToken = default);
    Task<double> GetStudentGpaAsync(Guid studentId, Guid? semesterId = null, CancellationToken cancellationToken = default);
}