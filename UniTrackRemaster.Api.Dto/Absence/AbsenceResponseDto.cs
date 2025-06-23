using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Absence;

public record AbsenceResponseDto(
    Guid Id,
    DateTime Date,
    AbsenceStatus Status,
    string? Reason,
    bool IsExcused,
    Guid StudentId,
    string StudentName,

    Guid? SubjectId,
    string? SubjectName,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static AbsenceResponseDto FromEntity(Data.Models.Academical.Absence absence) => new(
        absence.Id,
        absence.Date,
        absence.Status,
        absence.Reason,
        absence.IsExcused,
        absence.StudentId,
        $"{absence.Student.User?.FirstName} {absence.Student.User?.LastName}",
        absence.SubjectId,
        absence.Subject?.Name,
        absence.CreatedAt,
        absence.UpdatedAt
    );
}