using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Api.Dto.Subject;

public record StudentElectiveResponseDto(
    Guid Id,
    Guid StudentId,
    string StudentName,
    Guid SubjectId,
    string SubjectName,
    DateTime EnrollmentDate,
    string Status
)
{
    public static StudentElectiveResponseDto FromEntity(StudentElective enrollment) => new(
        enrollment.Id,
        enrollment.StudentId,
        enrollment.Student?.User?.FirstName + " " + enrollment.Student?.User?.LastName,
        enrollment.SubjectId,
        enrollment.Subject?.Name ?? "Unknown Subject",
        enrollment.EnrollmentDate,
        enrollment.Status.ToString()
    );
}