using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Semester;

public record SemesterResponseDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    Guid AcademicYearId,
    string AcademicYearName,
    SemesterType Type,
    SemesterStatus Status,
    string? Description,
    int WeekCount,
    DateTime? RegistrationStartDate,
    DateTime? RegistrationEndDate,
    DateTime? AddDropDeadline,
    DateTime? WithdrawalDeadline,
    DateTime? MidtermStartDate,
    DateTime? MidtermEndDate,
    DateTime? FinalExamStartDate,
    DateTime? FinalExamEndDate,
    DateTime? GradeSubmissionDeadline,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static SemesterResponseDto FromEntity(Data.Models.Academical.Semester semester) => new(
        semester.Id,
        semester.Name,
        semester.StartDate,
        semester.EndDate,
        semester.AcademicYearId,
        semester.AcademicYear?.Name ?? "Unknown Academic Year",
        semester.Type,
        semester.Status,
        semester.Description,
        semester.WeekCount,
        semester.RegistrationStartDate,
        semester.RegistrationEndDate,
        semester.AddDropDeadline,
        semester.WithdrawalDeadline,
        semester.MidtermStartDate,
        semester.MidtermEndDate,
        semester.FinalExamStartDate,
        semester.FinalExamEndDate,
        semester.GradeSubmissionDeadline,
        semester.CreatedAt,
        semester.UpdatedAt
    );
}
