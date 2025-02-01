using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Course;

public record UpdateCourseDto(
    string? Code,
    string? Name,
    string? Description,
    int? Credits,
    CourseType? Type);
