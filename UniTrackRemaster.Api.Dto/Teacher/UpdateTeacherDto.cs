namespace UniTrackRemaster.Api.Dto.Teacher;

public record UpdateTeacherDto(
    string? Title,
    Guid? ClassGradeId);