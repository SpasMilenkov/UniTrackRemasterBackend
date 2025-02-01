namespace UniTrackRemaster.Api.Dto.Major;

public record MajorResponseDto(
    Guid Id,
    string Name,
    string Code,
    string ShortDescription,
    string DetailedDescription,
    int RequiredCredits,
    int DurationInYears,
    string CareerOpportunities,
    string AdmissionRequirements,
    Guid FacultyId,
    string FacultyName,
    int StudentCount,
    int CourseCount,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static MajorResponseDto FromEntity(Data.Models.Academical.Major major) => new(
        major.Id,
        major.Name,
        major.Code,
        major.ShortDescription,
        major.DetailedDescription,
        major.RequiredCredits,
        major.DurationInYears,
        major.CareerOpportunities,
        major.AdmissionRequirements,
        major.FacultyId,
        major.Faculty.Name,
        major.Students?.Count ?? 0,
        major.Courses?.Count ?? 0,
        major.CreatedAt,
        major.UpdatedAt
    );
}
