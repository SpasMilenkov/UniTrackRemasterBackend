namespace UniTrackRemaster.Api.Dto.Metrics;


public record RequestMetricsDto(
    int TotalRequests,
    int SuccessfulRequests,
    int FailedRequests,
    double AverageResponseTime,
    Dictionary<string, int> RequestsByEndpoint,
    Dictionary<string, double> ResponseTimeByEndpoint
);