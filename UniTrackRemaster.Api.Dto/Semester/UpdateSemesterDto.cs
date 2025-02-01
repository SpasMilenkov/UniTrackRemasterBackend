namespace UniTrackRemaster.Api.Dto.Request;

public record UpdateSemesterDto(
    string? Name,
    DateTime? StartDate,
    DateTime? EndDate);
