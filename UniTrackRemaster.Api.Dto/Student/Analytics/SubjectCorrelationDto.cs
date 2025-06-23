namespace UniTrackRemaster.Api.Dto.Student.Analytics;


public record SubjectCorrelationDto(
    Guid SubjectId,
    string SubjectName,
    double AttendanceRate,
    decimal AverageScore,
    string PerformanceImpact
);
