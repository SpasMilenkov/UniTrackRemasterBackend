using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;

namespace UniTrackRemaster.Services.Student;

public interface IStudentService
{
    Task<StudentResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<StudentResponseDto>> GetBySchoolAsync(Guid schoolId);
    Task<IEnumerable<StudentResponseDto>> GetByUniversityAsync(Guid universityId);
    Task<IEnumerable<StudentResponseDto>> GetByGradeAsync(Guid gradeId);
    Task<StudentResponseDto> CreateAsync(CreateStudentDto dto);
    Task<StudentResponseDto> UpdateAsync(Guid id, UpdateStudentDto dto);
    Task DeleteAsync(Guid id);
}