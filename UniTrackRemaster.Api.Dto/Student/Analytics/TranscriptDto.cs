namespace UniTrackRemaster.Api.Dto.Student.Analytics;

public record TranscriptDto(
    List<CourseGradeDto> Courses,
    int TotalCreditsAttempted,
    int TotalCreditsEarned,
    int MajorCreditsCompleted,
    int MajorCreditsRequired,
    int GenEdCreditsCompleted,
    int GenEdCreditsRequired,
    Dictionary<string, double> GPAByTerm,
    Dictionary<string, SemesterSummaryDto> SemesterSummaries
);
