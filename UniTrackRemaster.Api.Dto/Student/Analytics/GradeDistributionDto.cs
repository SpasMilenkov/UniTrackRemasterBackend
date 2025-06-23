namespace UniTrackRemaster.Api.Dto.Student.Analytics;

public record GradeDistributionDto(
    Dictionary<string, int> GradeCounts
);