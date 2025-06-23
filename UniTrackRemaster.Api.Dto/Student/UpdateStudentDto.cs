namespace UniTrackRemaster.Api.Dto.Student;

public record UpdateStudentDto(
    bool? IsSchoolStudent,
    bool? IsUniversityStudent,
    Guid? SchoolId,
    Guid? UniversityId,
    Guid? GradeId);
