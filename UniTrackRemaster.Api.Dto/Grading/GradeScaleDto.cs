using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Api.Dto.Grading;

public record GradeScaleDto(
    [Required]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Grade should be between 1 and 20 characters.")]
        string Grade,

    [StringLength(200, ErrorMessage = "Description should not exceed 200 characters.")]
        string Description,

    [Required]
        [Range(0, 100, ErrorMessage = "Minimum score must be between 0 and 100.")]
        decimal MinimumScore,

    [Required]
        [Range(0, 100, ErrorMessage = "Maximum score must be between 0 and 100.")]
        decimal MaximumScore,

    [Required]
        [Range(0, 4.0, ErrorMessage = "GPA value must be between 0 and 4.0.")]
        double GpaValue)
{
    public static GradeScaleDto FromEntity(GradeScaleDto entity) => new(
        entity.Grade,
        entity.Description,
        entity.MinimumScore,
        entity.MaximumScore,
        entity.GpaValue
    );

    public static GradeScale ToEntity(GradeScaleDto dto, Guid gradingSystemId) => new()
    {
        Id = Guid.NewGuid(),
        Grade = dto.Grade,
        Description = dto.Description,
        MinimumScore = dto.MinimumScore,
        MaximumScore = dto.MaximumScore,
        GpaValue = dto.GpaValue,
        GradingSystemId = gradingSystemId
    };
}