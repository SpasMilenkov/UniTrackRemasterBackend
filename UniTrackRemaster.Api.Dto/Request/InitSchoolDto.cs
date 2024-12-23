using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Request;

public record InitSchoolDto(Guid Id, string Name, string Description, string Motto, string Website, string[] Programs)
{
    [Required]
    public required Guid Id { get; init; } = Id;

    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name should be between 2 and 200 characters.")]
    public required string Name { get; init; } = Name;

    [Required]
    [StringLength(500, ErrorMessage = "Description should not exceed 500 characters.")]
    public required string Description { get; init; } = Description;

    [Required]
    [StringLength(200, ErrorMessage = "Motto should not exceed 200 characters.")]
    public required string Motto { get; init; } = Motto;

    [Required]
    [Url(ErrorMessage = "Please enter a valid URL for the website.")]
    public required string Website { get; init; } = Website;

    [Required]
    [MinLength(1, ErrorMessage = "At least one program is required.")]
    public required string[] Programs { get; init; } = Programs;
}