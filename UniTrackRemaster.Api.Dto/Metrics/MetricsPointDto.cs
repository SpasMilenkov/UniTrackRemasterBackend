namespace UniTrackRemaster.Api.Dto.Metrics;

public record MetricPointDto(
     DateTimeOffset Timestamp,
     double Value,
     Dictionary<string, string> Labels
 );