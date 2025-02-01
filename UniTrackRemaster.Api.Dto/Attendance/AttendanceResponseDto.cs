using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Response;

public record AttendanceResponseDto(
    Guid Id,
    DateTime Date,
    AttendanceStatus Status,
    string? Reason,
    bool IsExcused,
    Guid StudentId,
    string StudentName,
    Guid? CourseId,
    string? CourseName,
    Guid? SubjectId,
    string? SubjectName,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static AttendanceResponseDto FromEntity(Attendance attendance) => new(
        attendance.Id,
        attendance.Date,
        attendance.Status,
        attendance.Reason,
        attendance.IsExcused,
        attendance.StudentId,
        $"{attendance.Student.User?.FirstName} {attendance.Student.User?.LastName}",
        attendance.CourseId,
        attendance.Course?.Name,
        attendance.SubjectId,
        attendance.Subject?.Name,
        attendance.CreatedAt,
        attendance.UpdatedAt
    );
}