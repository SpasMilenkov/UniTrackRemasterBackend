using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Services.Academics;

public interface IAttendanceService
{
    Task<AttendanceResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<AttendanceResponseDto>> GetByStudentAsync(Guid studentId);
    Task<IEnumerable<AttendanceResponseDto>> GetByCourseAsync(Guid courseId);
    Task<IEnumerable<AttendanceResponseDto>> GetBySubjectAsync(Guid subjectId);
    Task<AttendanceResponseDto> CreateAsync(CreateAttendanceDto dto);
    Task<AttendanceResponseDto> UpdateAsync(Guid id, UpdateAttendanceDto dto);
    Task DeleteAsync(Guid id);
}