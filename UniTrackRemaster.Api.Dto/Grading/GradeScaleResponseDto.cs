using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Api.Dto.Grading;

public record GradeScaleResponseDto
{
    public Guid Id { get; init; }
    public string Grade { get; init; }
    public string Description { get; init; }
    public decimal MinimumScore { get; init; }
    public decimal MaximumScore { get; init; }
    public double GpaValue { get; init; }

    public static GradeScaleResponseDto FromEntity(GradeScale entity)
    {
        return new GradeScaleResponseDto
        {
            Id = entity.Id,
            Grade = entity.Grade,
            Description = entity.Description,
            MinimumScore = entity.MinimumScore,
            MaximumScore = entity.MaximumScore,
            GpaValue = entity.GpaValue
        };
    }
}