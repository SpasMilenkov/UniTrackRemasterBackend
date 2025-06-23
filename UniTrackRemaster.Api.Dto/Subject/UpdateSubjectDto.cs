using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Subject;

public record UpdateSubjectDto(
    string? Name,
    string? Code,
    string? ShortDescription,
    string? DetailedDescription,
    // Type categorization
    SubjectType? SubjectType,
    // Educational context properties
    AcademicLevel? AcademicLevel,
    int? MinGradeLevel,
    int? MaxGradeLevel,
    // Credit information
    int? CreditHours,
    decimal? CreditValue,
    // Elective-specific properties
    bool? IsElective,
    ElectiveType? ElectiveType,
    int? MaxStudents,
    bool? HasLab,
    // Primary teacher and department
    Guid? PrimaryTeacherId,
    Guid? DepartmentId,
    // Collections for direct relationships
    IEnumerable<Guid>? TeacherIds,
    IEnumerable<Guid>? GradeIds
);

