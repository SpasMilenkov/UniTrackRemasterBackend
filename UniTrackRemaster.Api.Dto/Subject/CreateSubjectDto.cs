using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Request;

public record CreateSubjectDto(
    [Required] string Name,
    [Required] string ShortDescription,
    string? DetailedDescription);