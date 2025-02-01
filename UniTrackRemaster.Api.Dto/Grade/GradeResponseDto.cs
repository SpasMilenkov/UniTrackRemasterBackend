namespace UniTrackRemaster.Api.Dto.Grade;

public record GradeResponseDto(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static GradeResponseDto FromEntity(Data.Models.Academical.Grade  grade) => new(
        grade.Id,
        grade.Name,
        grade.CreatedAt,
        grade.UpdatedAt
    );
}
