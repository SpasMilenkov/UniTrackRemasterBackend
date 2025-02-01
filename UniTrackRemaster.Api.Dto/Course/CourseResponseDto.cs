using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Course;

public record CourseResponseDto(
    Guid Id,
    string Code,
    string Name,
    string Description,
    int Credits,
    CourseType Type,
    Guid SubjectId,
    string SubjectName,
    Guid? MajorId,
    string? MajorName,
    Guid SemesterId,
    string SemesterName,
    int EnrolledStudentsCount,
    int AssignmentsCount,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static CourseResponseDto FromEntity(Data.Models.Academical.Course course) => new(
        course.Id,
        course.Code,
        course.Name,
        course.Description,
        course.Credits,
        course.Type,
        course.SubjectId,
        course.Subject.Name,
        course.MajorId,
        course.Major?.Name,
        course.SemesterId,
        course.Semester.Name,
        course.StudentCourses?.Count ?? 0,
        course.Assignments?.Count ?? 0,
        course.CreatedAt,
        course.UpdatedAt
    );
}
