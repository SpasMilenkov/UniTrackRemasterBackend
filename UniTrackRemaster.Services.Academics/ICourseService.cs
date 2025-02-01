using UniTrackRemaster.Api.Dto.Course;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;

namespace UniTrackRemaster.Services.Academics;

public interface ICourseService
{
    Task<CourseResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<CourseResponseDto>> GetBySemesterAsync(Guid semesterId);
    Task<IEnumerable<CourseResponseDto>> GetBySubjectAsync(Guid subjectId);
    Task<IEnumerable<CourseResponseDto>> GetByMajorAsync(Guid majorId);
    Task<CourseResponseDto> CreateAsync(CreateCourseDto dto);
    Task<CourseResponseDto> UpdateAsync(Guid id, UpdateCourseDto dto);
    Task DeleteAsync(Guid id);
}