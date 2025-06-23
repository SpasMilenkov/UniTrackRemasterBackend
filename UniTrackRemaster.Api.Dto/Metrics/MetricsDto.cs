namespace UniTrackRemaster.Api.Dto.Metrics;

public record MetricDto(
    string Name,
    string Description,
    string Unit,
    List<MetricPointDto> Points
);