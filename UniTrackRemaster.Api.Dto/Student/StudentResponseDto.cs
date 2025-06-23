using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Student;

public record StudentResponseDto(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    bool IsSchoolStudent,
    Guid? SchoolId,
    string? SchoolName,
    Guid? UniversityId,
    string? UniversityName,
    Guid GradeId,
    string GradeName,
    int? MarkCount,
    int? AttendanceCount,
    int? ElectivesCount,
    ProfileStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static StudentResponseDto FromEntity(Data.Models.Users.Student student) => new(
        student.Id,
        student.User?.Id ?? Guid.Empty,
        student.User?.FirstName ?? "",
        student.User?.LastName ?? "",
        student.User?.Email ?? "",
        student.IsSchoolStudent,
        student.SchoolId,
        student.School?.Institution?.Name,
        student.UniversityId,
        student.University?.Institution?.Name,
        student.GradeId,
        student.Grade?.Name ?? "",
        student.Marks?.Count,
        student.AttendanceRecords?.Count,
        student.Electives?.Count,
        student.Status, // Added Status
        student.CreatedAt,
        student.UpdatedAt
    );
}