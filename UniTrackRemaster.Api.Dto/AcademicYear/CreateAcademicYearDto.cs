using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Request;

public record CreateAcademicYearDto(
    [Required] string Name,
    [Required] DateTime StartDate,
    [Required] DateTime EndDate,
    [Required] Guid InstitutionId);