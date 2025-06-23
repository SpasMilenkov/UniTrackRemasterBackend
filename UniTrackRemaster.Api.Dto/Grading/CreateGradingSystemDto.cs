using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Grading;

public record CreateGradingSystemDto(
    [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name should be between 2 and 100 characters.")]
        string Name,

    [StringLength(500, ErrorMessage = "Description should not exceed 500 characters.")]
        string Description,

    [Required]
        GradingSystemType Type,

    bool IsDefault,

    [Required]
        [Range(0, 100, ErrorMessage = "Minimum passing score must be between 0 and 100.")]
        decimal MinimumPassingScore,

    [Required]
        [Range(0, 100, ErrorMessage = "Maximum score must be between 0 and 100.")]
        decimal MaximumScore,

    [Required]
        Guid InstitutionId,

    List<GradeScaleDto> GradeScales = null)
{
    public static GradingSystem ToEntity(CreateGradingSystemDto dto)
    {
        var gradingSystem = new GradingSystem
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Type = dto.Type,
            IsDefault = dto.IsDefault,
            MinimumPassingScore = dto.MinimumPassingScore,
            MaximumScore = dto.MaximumScore,
            InstitutionId = dto.InstitutionId,
            GradeScales = new List<GradeScale>()
        };

        if (dto.GradeScales != null)
        {
            foreach (var scaleDto in dto.GradeScales)
            {
                gradingSystem.GradeScales.Add(GradeScaleDto.ToEntity(scaleDto, gradingSystem.Id));
            }
        }

        return gradingSystem;
    }
}