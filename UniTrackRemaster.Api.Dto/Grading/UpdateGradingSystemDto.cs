using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Grading;

public record UpdateGradingSystemDto(
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name should be between 2 and 100 characters.")]
        string Name,

    [StringLength(500, ErrorMessage = "Description should not exceed 500 characters.")]
        string Description,

    bool? IsDefault,

    [Range(0, 100, ErrorMessage = "Minimum passing score must be between 0 and 100.")]
        decimal? MinimumPassingScore,

    [Range(0, 100, ErrorMessage = "Maximum score must be between 0 and 100.")]
        decimal? MaximumScore,

    List<GradeScaleDto> GradeScales = null);