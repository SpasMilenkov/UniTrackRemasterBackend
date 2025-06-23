namespace UniTrackRemaster.Api.Dto.Metrics;

public record UserStatisticsDto(
    int TotalUsers,
    int ActiveUsers,
    int AdminCount,
    int TeacherCount,
    int StudentCount,
    int ParentCount,
    Dictionary<string, int> UsersByRole
);
