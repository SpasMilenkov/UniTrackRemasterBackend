namespace UniTrackRemaster.Api.Dto.Metrics;

public record EntityCountsDto(
    DateTimeOffset CollectedAt,
    List<EntityCountDto> Entities
);