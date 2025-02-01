using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Academical;

public record CreateMajorDto(
    [Required] string Name,
    [Required] string Code,
    [Required] string ShortDescription,
    string DetailedDescription,
    [Range(0, 300)] int RequiredCredits,
    [Range(1, 10)] int DurationInYears,
    string CareerOpportunities,
    string AdmissionRequirements,
    [Required] Guid FacultyId);
