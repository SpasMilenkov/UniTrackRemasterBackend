using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Api.Dto.Response;

public record SubjectResponseDto(
    Guid Id,
    string Name,
    string ShortDescription,
    string? DetailedDescription,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static SubjectResponseDto FromEntity(Subject subject) => new(
        subject.Id,
        subject.Name,
        subject.ShortDescription,
        subject.DetailedDescription,
        subject.CreatedAt,
        subject.UpdatedAt
    );
}