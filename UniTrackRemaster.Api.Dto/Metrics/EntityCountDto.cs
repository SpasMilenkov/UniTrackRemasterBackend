namespace UniTrackRemaster.Api.Dto.Metrics;

public record EntityCountDto(
    string EntityName,
    int TotalCount,
    int ActiveCount,
    DateTime LastUpdated
);