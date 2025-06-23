namespace UniTrackRemaster.Api.Dto.Student.Analytics;

public record CourseGradeDto(
    Guid Id,
    string Code,
    string Name,
    string Instructor,
    decimal Score,
    string Grade,
    int? Credits,
    string Term,
    Guid? SemesterId,
    string SemesterName,
    string Status
);
