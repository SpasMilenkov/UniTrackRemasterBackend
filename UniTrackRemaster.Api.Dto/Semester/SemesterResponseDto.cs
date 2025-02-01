using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Api.Dto.Response;

public record SemesterResponseDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    Guid AcademicYearId,
    string AcademicYearName,
    int CourseCount,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static SemesterResponseDto FromEntity(Semester semester) => new(
        semester.Id,
        semester.Name,
        semester.StartDate,
        semester.EndDate,
        semester.AcademicYearId,
        semester.AcademicYear.Name,
        semester.Courses?.Count ?? 0,
        semester.CreatedAt,
        semester.UpdatedAt
    );
}