using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Course;

public record CreateCourseDto(
    [Required] string Code,
    [Required] string Name,
    string Description,
    [Range(1, 30)] int Credits,
    [Required] CourseType Type,
    [Required] Guid SubjectId,
    Guid? MajorId,
    [Required] Guid SemesterId);
