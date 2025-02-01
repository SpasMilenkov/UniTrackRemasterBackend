namespace UniTrackRemaster.Api.Dto.Request;


public record UpdateAcademicYearDto(
    string? Name,
    DateTime? StartDate,
    DateTime? EndDate);