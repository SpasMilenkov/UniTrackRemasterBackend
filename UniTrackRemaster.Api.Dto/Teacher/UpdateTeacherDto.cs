namespace UniTrackRemaster.Api.Dto.Request;

public record UpdateTeacherDto(
    string? Title,
    Guid? ClassGradeId);