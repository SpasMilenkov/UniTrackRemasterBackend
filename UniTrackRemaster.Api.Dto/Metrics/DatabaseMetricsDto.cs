namespace UniTrackRemaster.Api.Dto.Metrics;

public record DatabaseMetricsDto(
    int ActiveConnections,
    int MaxConnections,
    double AverageQueryTime,
    int QueryCount,
    int ErrorCount
);
