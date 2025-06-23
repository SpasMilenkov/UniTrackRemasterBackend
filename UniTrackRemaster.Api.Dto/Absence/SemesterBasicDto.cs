using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Absence;

public record SemesterBasicDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    SemesterType Type,
    SemesterStatus Status)
{
    public static SemesterBasicDto FromEntity(Data.Models.Academical.Semester semester) => new(
        semester.Id,
        semester.Name,
        semester.StartDate,
        semester.EndDate,
        semester.Type,
        semester.Status
    );
}