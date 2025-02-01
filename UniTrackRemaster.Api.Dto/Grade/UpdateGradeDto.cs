using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Grade;

public record UpdateGradeDto(
    [Required]
    [StringLength(50, MinimumLength = 1)]
    string Name);
