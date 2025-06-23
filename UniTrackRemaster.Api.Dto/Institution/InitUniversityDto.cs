using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Institution;

public record InitUniversityDto(
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
    DateTime EstablishedDate,
    [Required]
    [MinLength(1, ErrorMessage = "At least one focus area is required.")]
    FocusArea[] FocusAreas,
    [Required]
    [Range(0, 100000, ErrorMessage = "Undergraduate count must be between 0 and 100,000.")]
    int UndergraduateCount,
    [Required]
    [Range(0, 50000, ErrorMessage = "Graduate count must be between 0 and 50,000.")]
    int GraduateCount,
    [Required]
    [Range(0, 100, ErrorMessage = "Acceptance rate must be between 0 and 100.")]
    double AcceptanceRate,
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Research funding must be a positive number.")]
    int ResearchFunding,
    [Required]
    bool HasStudentHousing,
    [Required]
    [MinLength(1, ErrorMessage = "At least one department is required.")]
    string[] Departments,
    [Required]
    InstitutionType Type)
{
    private List<FocusArea> focusAreas;
    private List<string> departments;
}

