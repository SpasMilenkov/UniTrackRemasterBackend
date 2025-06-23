using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.University;

// public record UniversityResponseDto();
/// <summary>
/// DTO for University response
/// </summary>
public record UniversityResponseDto(
    Guid Id,
    Guid InstitutionId,
    string Name,
    string Description,
    string Motto,
    string Website,
    DateTime EstablishedDate,
    IList<FocusArea> FocusAreas,
    int UndergraduateCount,
    int GraduateCount,
    double AcceptanceRate,
    int ResearchFunding,
    bool HasStudentHousing,
    IList<string> Departments,
    int FacultiesCount,
    string Email,
    string Phone
)
{
    /// <summary>
    /// Convert from entity to DTO
    /// </summary>
    public static UniversityResponseDto FromEntity(Data.Models.Organizations.University university)
    {
        return new UniversityResponseDto(
            university.Id,
            university.InstitutionId,
            university.Institution?.Name ?? string.Empty,
            university.Institution?.Description ?? string.Empty,
            university.Institution?.Motto ?? string.Empty,
            university.Institution?.Website ?? string.Empty,
            university.Institution?.EstablishedDate ?? DateTime.MinValue,
            university.FocusAreas ?? new List<FocusArea>(),
            university.UndergraduateCount,
            university.GraduateCount,
            university.AcceptanceRate,
            university.ResearchFunding,
            university.HasStudentHousing,
            university.Departments ?? new List<string>(),
            university.Faculties?.Count ?? 0,
            university.Institution?.Email ?? string.Empty,
            university.Institution?.Phone ?? string.Empty
        );
    }
}

