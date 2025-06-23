using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.University;

/// <summary>
/// DTO for updating an existing University
/// </summary>
public record UpdateUniversityDto(
    IList<FocusArea>? FocusAreas = null,
    int? UndergraduateCount = null,
    int? GraduateCount = null,
    double? AcceptanceRate = null,
    int? ResearchFunding = null,
    bool? HasStudentHousing = null,
    IList<string>? Departments = null
);