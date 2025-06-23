namespace UniTrackRemaster.Api.Dto.Metrics;

public record HealthMetricDto(
    double CpuUsage,
    double MemoryUsage,
    int ActiveRequests,
    double RequestsPerSecond,
    double AverageResponseTime
);