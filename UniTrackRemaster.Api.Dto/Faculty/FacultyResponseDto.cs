using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Faculty;


public record FacultyResponseDto(
    Guid Id,
    string Name,
    string Code,
    string ShortDescription,
    string DetailedDescription,
    string? Website,
    string? ContactEmail,
    string? ContactPhone,
    FacultyStatus Status,
    Guid UniversityId,
    string UniversityName,
    int MajorsCount,
    int DepartmentsCount,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static FacultyResponseDto FromEntity(Data.Models.Academical.Faculty faculty) => new(
        faculty.Id,
        faculty.Name,
        faculty.Code,
        faculty.ShortDescription,
        faculty.DetailedDescription,
        faculty.Website,
        faculty.ContactEmail,
        faculty.ContactPhone,
        faculty.Status,
        faculty.UniversityId,
        faculty.University.Institution.Name,
        faculty.Majors?.Count ?? 0,
        faculty.Departments?.Count ?? 0,
        faculty.CreatedAt,
        faculty.UpdatedAt
    );
}