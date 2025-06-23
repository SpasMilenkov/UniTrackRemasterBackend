using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.AcademicYear;

public record CreateAcademicYearDto(
    [Required] string Name,
    [Required] DateTime StartDate,
    [Required] DateTime EndDate,
    [Required] Guid InstitutionId);