using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Grade;

public record CreateGradeDto(
    [Required]
    [StringLength(50, MinimumLength = 1)]
    string Name,
    
    [Required]
    Guid InstitutionId,
    
    [Required]
    Guid AcademicYearId,
    
    Guid? HomeRoomTeacherId = null);