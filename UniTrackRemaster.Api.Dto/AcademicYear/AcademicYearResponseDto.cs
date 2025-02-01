namespace UniTrackRemaster.Api.Dto.AcademicYear;

public record AcademicYearResponseDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    Guid InstitutionId,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static AcademicYearResponseDto FromEntity(Data.Models.Academical.AcademicYear year) => new(
        year.Id,
        year.Name,
        year.StartDate,
        year.EndDate,
        year.InstitutionId,
        year.CreatedAt,
        year.UpdatedAt
    );
}

