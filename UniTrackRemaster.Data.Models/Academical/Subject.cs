using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Organizations;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Academical;

public class Subject: BaseEntity
{
    #region Properties
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }  // Subject code (e.g., MATH101, ENG203)
    public required string ShortDescription { get; set; }
    public string? DetailedDescription { get; set; }
    
    // Type categorization
    public SubjectType SubjectType { get; set; }
    
    // Educational context properties
    public AcademicLevel? AcademicLevel { get; set; }
    public int? MinGradeLevel { get; set; }  // For K-12: minimum grade
    public int? MaxGradeLevel { get; set; }  // For K-12: maximum grade
    
    // Credit information
    public int? CreditHours { get; set; }
    public decimal? CreditValue { get; set; }
    
    // Elective-specific properties
    public bool IsElective { get; set; }
    public ElectiveType? ElectiveType { get; set; }
    public int? MaxStudents { get; set; }  // Only relevant for electives
    public bool HasLab { get; set; }
    
    // Primary teacher for electives, optional
    public Guid? PrimaryTeacherId { get; set; }
    public Teacher? PrimaryTeacher { get; set; }
    
    // Department link (for higher education)
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    #endregion
    
    #region NavigationProperties
    public ICollection<Grade> Grades { get; set; }
    public ICollection<Teacher> Teachers { get; set; }
    // For elective tracking
    public ICollection<StudentElective> StudentElectives { get; set; }
    public Institution? Institution { get; set; }
    public Guid InstitutionId { get; set; }
    #endregion
}
