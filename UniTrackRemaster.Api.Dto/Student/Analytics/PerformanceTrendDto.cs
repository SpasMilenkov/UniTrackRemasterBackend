namespace UniTrackRemaster.Api.Dto.Student.Analytics;

public record PerformanceTrendDto(
    List<string> Terms,
    List<double> StudentAverages,
    List<double> ClassAverages,
    List<Guid> SemesterIds
);