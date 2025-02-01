using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Department;

public record DepartmentResponseDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    string? Location,
    string? ContactEmail,
    string? ContactPhone,
    DepartmentType Type,
    DepartmentStatus Status,
    Guid FacultyId,
    string FacultyName,
    int TeachersCount,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static DepartmentResponseDto FromEntity(Data.Models.Academical.Department department) => new(
        department.Id,
        department.Name,
        department.Code,
        department.Description,
        department.Location,
        department.ContactEmail,
        department.ContactPhone,
        department.Type,
        department.Status,
        department.FacultyId,
        department.Faculty.Name,
        department.Teachers?.Count ?? 0,
        department.CreatedAt,
        department.UpdatedAt
    );
}
