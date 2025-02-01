using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Institution;

public record InitSchoolDto(
    [Required]
    Guid Id, 
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name should be between 2 and 200 characters.")]
    string Name,
    [Required]
    [StringLength(500, ErrorMessage = "Description should not exceed 500 characters.")]
    string Description,
    [Required]
    [StringLength(200, ErrorMessage = "Motto should not exceed 200 characters.")]
    string Motto,
    [Required]
    [Url(ErrorMessage = "Please enter a valid URL for the website.")]
    string Website,
    [Required]
    [StringLength(50, ErrorMessage = "School type should not exceed 50 characters.")]
    string Type,
    [Required]
    DateTime EstablishedDate,
    [Required]
    [MinLength(1, ErrorMessage = "At least one program is required.")]
    string[] Programs);