namespace UniTrackRemaster.Api.Dto.Student;

public record StudentsByGradeDto(
    Guid GradeId,
    string GradeName,
    IEnumerable<StudentResponseDto> Students
);