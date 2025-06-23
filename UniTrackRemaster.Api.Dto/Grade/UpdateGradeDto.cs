using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Grade;

public record UpdateGradeDto(
    [StringLength(50, MinimumLength = 1)]
    string? Name = null,
    
    Guid? InstitutionId = null,
    
    Guid? AcademicYearId = null,
    
    Guid? HomeRoomTeacherId = null);