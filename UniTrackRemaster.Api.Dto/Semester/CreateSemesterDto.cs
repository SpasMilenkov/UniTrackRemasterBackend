using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Semester;

public record CreateSemesterDto(
    [Required] string Name,
    [Required] DateTime StartDate,
    [Required] DateTime EndDate,
    [Required] Guid AcademicYearId,
    [Required] SemesterType Type,
    SemesterStatus Status = SemesterStatus.Upcoming,
    string? Description = null,
    int WeekCount = 0,
    DateTime? RegistrationStartDate = null,
    DateTime? RegistrationEndDate = null,
    DateTime? AddDropDeadline = null,
    DateTime? WithdrawalDeadline = null,
    DateTime? MidtermStartDate = null,
    DateTime? MidtermEndDate = null,
    DateTime? FinalExamStartDate = null,
    DateTime? FinalExamEndDate = null,
    DateTime? GradeSubmissionDeadline = null)
{
    public static Data.Models.Academical.Semester ToEntity(CreateSemesterDto dto) => new()
    {
        Id = Guid.NewGuid(),
        Name = dto.Name,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        AcademicYearId = dto.AcademicYearId,
        Type = dto.Type,
        Status = dto.Status,
        Description = dto.Description,
        WeekCount = dto.WeekCount,
        RegistrationStartDate = dto.RegistrationStartDate,
        RegistrationEndDate = dto.RegistrationEndDate,
        AddDropDeadline = dto.AddDropDeadline,
        WithdrawalDeadline = dto.WithdrawalDeadline,
        MidtermStartDate = dto.MidtermStartDate,
        MidtermEndDate = dto.MidtermEndDate,
        FinalExamStartDate = dto.FinalExamStartDate,
        FinalExamEndDate = dto.FinalExamEndDate,
        GradeSubmissionDeadline = dto.GradeSubmissionDeadline
    };
}