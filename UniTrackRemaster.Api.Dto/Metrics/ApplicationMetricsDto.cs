namespace UniTrackRemaster.Api.Dto.Metrics;

public record ApplicationMetricsDto(
     DateTimeOffset Timestamp,
     List<MetricDto> Metrics
 );
