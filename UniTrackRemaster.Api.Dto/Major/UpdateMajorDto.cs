namespace UniTrackRemaster.Api.Dto.Major;

public record UpdateMajorDto(
    string? Name,
    string? Code,
    string? ShortDescription,
    string? DetailedDescription,
    int? RequiredCredits,
    int? DurationInYears,
    string? CareerOpportunities,
    string? AdmissionRequirements);