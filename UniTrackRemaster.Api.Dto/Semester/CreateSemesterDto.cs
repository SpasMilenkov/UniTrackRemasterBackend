using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Request;

public record CreateSemesterDto(
    [Required] string Name,
    [Required] DateTime StartDate,
    [Required] DateTime EndDate,
    [Required] Guid AcademicYearId);