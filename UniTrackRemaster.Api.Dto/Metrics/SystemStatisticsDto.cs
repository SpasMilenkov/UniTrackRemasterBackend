namespace UniTrackRemaster.Api.Dto.Metrics;

public record SystemStatisticsDto(
    DateTimeOffset CollectedAt,
    UserStatisticsDto Users,
    AcademicStatisticsDto Academic,
    ActivityStatisticsDto Activity
);