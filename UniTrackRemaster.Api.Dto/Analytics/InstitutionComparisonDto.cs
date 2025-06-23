namespace UniTrackRemaster.Api.Dto.Analytics;

public record InstitutionComparisonDto(
    InstitutionSummaryDto Institution1,
    InstitutionSummaryDto Institution2,
    string ComparisonAnalysis,
    DateTime GeneratedAt
);