namespace UniTrackRemaster.Api.Dto.Student.Analytics;

public record SemesterSummaryDto(
    Guid SemesterId,
    string SemesterName,
    string SemesterType,
    string AcademicYear,
    double GPA,
    int Credits,
    List<CourseGradeDto> Courses
);
