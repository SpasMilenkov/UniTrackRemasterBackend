using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Grading;

public record GradingSystemResponseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public GradingSystemType Type { get; init; }
    public bool IsDefault { get; init; }
    public decimal MinimumPassingScore { get; init; }
    public decimal MaximumScore { get; init; }
    public Guid InstitutionId { get; init; }
    public List<GradeScaleResponseDto> GradeScales { get; init; } = [];

    public static GradingSystemResponseDto FromEntity(GradingSystem entity)
    {
        return new GradingSystemResponseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Type = entity.Type,
            IsDefault = entity.IsDefault,
            MinimumPassingScore = entity.MinimumPassingScore,
            MaximumScore = entity.MaximumScore,
            InstitutionId = entity.InstitutionId,
            GradeScales = entity.GradeScales?
                .Select(gs => GradeScaleResponseDto.FromEntity(gs))
                .ToList() ?? []
        };
    }
}

