using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Request;

public record CreateSchoolDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name should be between 2 and 200 characters.")]
    public required string Name { get; init; }

    [Required]
    [StringLength(500, ErrorMessage = "Description should not exceed 500 characters.")]
    public required string Description { get; init; }

    [Required]
    [StringLength(200, ErrorMessage = "Motto should not exceed 200 characters.")]
    public required string Motto { get; init; }

    [Required]
    [Url(ErrorMessage = "Please enter a valid URL for the website.")]
    public required string Website { get; init; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one program is required.")]
    public required string[] Programs { get; init; }
}