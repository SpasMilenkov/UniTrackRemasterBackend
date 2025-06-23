namespace UniTrackRemaster.Api.Dto.Student.Analytics;

public record StudentGradeDashboardDto(
    double GPA,
    double GPATrend,
    int? ClassRank,
    double? ClassAverage,
    List<CourseGradeDto> Courses,
    List<TeacherCommentDto> Comments,
    GradeDistributionDto GradeDistribution,
    PerformanceTrendDto PerformanceTrend,
    Guid? CurrentSemesterId,
    string CurrentSemesterName,
    List<SemesterSummaryDto> SemesterBreakdown
);
