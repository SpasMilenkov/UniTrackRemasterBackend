namespace UniTrackRemaster.Api.Dto.Analytics;

public record InstitutionSummaryDto(
    Guid InstitutionId,
    string Name,
    decimal OverallScore
);
