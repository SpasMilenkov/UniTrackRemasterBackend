namespace UniTrackRemaster.Api.Dto.AcademicYear;


public record UpdateAcademicYearDto(
    string? Name,
    DateTime? StartDate,
    DateTime? EndDate);