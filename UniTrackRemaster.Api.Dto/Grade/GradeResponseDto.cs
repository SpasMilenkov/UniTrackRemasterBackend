namespace UniTrackRemaster.Api.Dto.Grade;

// GradeResponseDto.cs
public record GradeResponseDto(
    Guid Id,
    string Name,
    Guid InstitutionId,
    string InstitutionName,
    Guid AcademicYearId,
    string AcademicYearName,
    Guid? HomeRoomTeacherId,
    string HomeRoomTeacherName,
    int StudentsCount,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static GradeResponseDto FromEntity(Data.Models.Academical.Grade grade) => new(
        grade.Id,
        grade.Name,
        grade.InstitutionId,
        grade.Institution?.Name ?? string.Empty,
        grade.AcademicYearId,
        grade.AcademicYear?.Name ?? string.Empty,
        grade.HomeRoomTeacherId != Guid.Empty ? grade.HomeRoomTeacherId : null,
        grade.HomeRoomTeacher?.Teacher?.User?.FirstName + " " + grade.HomeRoomTeacher?.Teacher?.User?.LastName,
        grade.Students?.Count ?? 0,
        grade.CreatedAt,
        grade.UpdatedAt
    );
}