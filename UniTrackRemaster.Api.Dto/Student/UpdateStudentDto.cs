namespace UniTrackRemaster.Api.Dto.Request;

public record UpdateStudentDto(
    bool? IsSchoolStudent,
    bool? IsUniversityStudent,
    Guid? SchoolId,
    Guid? UniversityId,
    Guid? GradeId);
