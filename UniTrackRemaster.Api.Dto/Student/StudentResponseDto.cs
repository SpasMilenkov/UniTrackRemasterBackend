using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Api.Dto.Response;

public record StudentResponseDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsSchoolStudent,
    bool IsUniversityStudent,
    Guid? SchoolId,
    string? SchoolName,
    Guid? UniversityId,
    string? UniversityName,
    Guid GradeId,
    string GradeName,
    int? MarkCount,
    int? AttendanceCount,
    int? ClubMembershipsCount,
    int? ElectivesCount,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static StudentResponseDto FromEntity(Student student) => new(
        student.Id,
        student.User?.FirstName ?? "",
        student.User?.LastName ?? "",
        student.User?.Email ?? "",
        student.IsSchoolStudent,
        student.IsUniversityStudent,
        student.SchoolId,
        student.School?.Institution.Name,
        student.UniversityId,
        student.University?.Institution.Name,
        student.GradeId,
        student.Grade?.Name ?? "",
        student.Marks?.Count,
        student.AttendanceRecords?.Count,
        student.ClubMemberships?.Count,
        student.Electives?.Count,
        student.CreatedAt,
        student.UpdatedAt
    );
}