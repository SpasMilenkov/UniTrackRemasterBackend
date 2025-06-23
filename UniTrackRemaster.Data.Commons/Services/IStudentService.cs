using UniTrackRemaster.Api.Dto.Student;

namespace UniTrackRemaster.Commons.Services;

public interface IStudentService
{
    Task<StudentResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<StudentResponseDto>> GetBySchoolAsync(Guid schoolId);
    Task<IEnumerable<StudentResponseDto>> GetByUniversityAsync(Guid universityId);
    Task<IEnumerable<StudentResponseDto>> GetByGradeAsync(Guid gradeId);
    Task<StudentResponseDto> CreateAsync(CreateStudentDto dto);
    Task<StudentResponseDto> UpdateAsync(Guid id, UpdateStudentDto dto);
    Task DeleteAsync(Guid id);
    Task<StudentResponseDto> GetByUserIdAsync(Guid userId);
    Task<PaginatedStudentResponseDto> SearchWithPaginationAsync(StudentSearchRequestDto request);
}