namespace UniTrackRemaster.Api.Dto.Request;

public record UpdateSubjectDto(
    string? Name,
    string? ShortDescription,
    string? DetailedDescription);