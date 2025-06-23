using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Subject;

public record SubjectResponseDto(
    Guid Id,
    string Name,
    string Code,
    string ShortDescription,
    string? DetailedDescription,
    
    // Type categorization
    SubjectType SubjectType,
    
    // Educational context properties
    AcademicLevel? AcademicLevel,
    int? MinGradeLevel,
    int? MaxGradeLevel,
    
    // Credit information
    int? CreditHours,
    decimal? CreditValue,
    
    // Elective-specific properties
    bool IsElective,
    ElectiveType? ElectiveType,
    int? MaxStudents,
    bool HasLab,
    
    // Primary teacher and department
    Guid? PrimaryTeacherId,
    string? PrimaryTeacherName,
    Guid? DepartmentId,
    string? DepartmentName,
    
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static SubjectResponseDto FromEntity(Data.Models.Academical.Subject subject) => new(
        subject.Id,
        subject.Name,
        subject.Code,
        subject.ShortDescription,
        subject.DetailedDescription,
        subject.SubjectType,
        subject.AcademicLevel,
        subject.MinGradeLevel,
        subject.MaxGradeLevel,
        subject.CreditHours,
        subject.CreditValue,
        subject.IsElective,
        subject.ElectiveType,
        subject.MaxStudents,
        subject.HasLab,
        subject.PrimaryTeacherId,
        subject.PrimaryTeacher?.User?.FirstName + ' ' + subject.PrimaryTeacher?.User?.FirstName, // Assuming Teacher has a FullName property
        subject.DepartmentId,
        subject.Department?.Name,
        subject.CreatedAt,
        subject.UpdatedAt
    );
}