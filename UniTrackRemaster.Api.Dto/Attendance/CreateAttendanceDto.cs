using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Request;

public record CreateAttendanceDto(
    [Required] DateTime Date,
    [Required] AttendanceStatus Status,
    string? Reason,
    bool IsExcused,
    [Required] Guid StudentId,
    Guid? CourseId,
    Guid? SubjectId)
{
    public bool Validate()
    {
        return (CourseId.HasValue && !SubjectId.HasValue) || 
               (!CourseId.HasValue && SubjectId.HasValue);
    }
}

