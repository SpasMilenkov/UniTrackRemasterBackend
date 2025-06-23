using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Semester;

public record UpdateSemesterDto(
    string? Name = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    SemesterType? Type = null,
    SemesterStatus? Status = null,
    string? Description = null,
    int? WeekCount = null,
    DateTime? RegistrationStartDate = null,
    DateTime? RegistrationEndDate = null,
    DateTime? AddDropDeadline = null,
    DateTime? WithdrawalDeadline = null,
    DateTime? MidtermStartDate = null,
    DateTime? MidtermEndDate = null,
    DateTime? FinalExamStartDate = null,
    DateTime? FinalExamEndDate = null,
    DateTime? GradeSubmissionDeadline = null);
