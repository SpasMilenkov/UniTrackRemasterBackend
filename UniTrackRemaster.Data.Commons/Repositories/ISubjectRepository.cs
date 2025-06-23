using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Commons.Repositories;

/// <summary>
/// Data for elective enrollments with subject details
/// </summary>
public class ElectiveEnrollmentsData
{
    public bool SubjectNotFound { get; set; }
    public bool IsElective { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public List<StudentElective> Enrollments { get; set; } = new();
}

/// <summary>
/// Data for student electives
/// </summary>
public class StudentElectivesData
{
    public bool StudentNotFound { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public List<Subject> Subjects { get; set; } = new();
}

/// <summary>
/// Students data with teacher validation
/// </summary>
public class SubjectStudentsData
{
    public bool TeacherTeachesSubject { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public List<Student> Students { get; set; } = new();
}

/// <summary>
/// Subjects data with teacher validation
/// </summary>
public class TeacherSubjectsData
{
    public bool TeacherNotFound { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public List<Subject> Subjects { get; set; } = new();
}

/// <summary>
/// Students by grade data with additional validations
/// </summary>
public class SubjectStudentsByGradeData
{
    public bool SubjectNotFound { get; set; }
    public bool TeacherTeachesSubject { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public bool IsElective { get; set; }
    public List<StudentGradeGroup> StudentsByGrade { get; set; } = new();
}

/// <summary>
/// Validation result for elective enrollment
/// </summary>
public class ElectiveEnrollmentValidation
{
    public bool IsElective { get; set; }
    public bool StudentNotFound { get; set; }
    public bool AlreadyEnrolled { get; set; }
    public bool MaxCapacityReached { get; set; }
    public int CurrentEnrollment { get; set; }
    public int? MaxEnrollment { get; set; }
}
// ISubjectRepository.cs - Interface
public interface ISubjectRepository
{
    // Basic CRUD operations
    Task<Subject?> GetByIdAsync(Guid id);
    Task<Subject?> GetByIdWithRelationsAsync(Guid id);
    Task<Subject> CreateAsync(Subject subject);
    Task UpdateAsync(Subject subject);
    Task DeleteAsync(Guid id);
    
    // Basic count and exists methods
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    // Non-paginated methods (for calculations and internal operations)
    Task<IEnumerable<Subject>> GetAllAsync();
    Task<IEnumerable<Subject>> GetAllWithRelationsAsync();
    Task<IEnumerable<Subject>> GetByDepartmentAsync(Guid departmentId);
    Task<IEnumerable<Subject>> GetByDepartmentWithRelationsAsync(Guid departmentId);
    Task<IEnumerable<Subject>> GetElectivesAsync(bool activeOnly = true);
    Task<IEnumerable<Subject>> GetElectivesWithRelationsAsync(bool activeOnly);
    Task<IEnumerable<Subject>> SearchAsync(string searchTerm);
    Task<IEnumerable<Subject>> SearchWithRelationsAsync(string searchTerm);

    // Paginated methods with filtering (for API endpoints)
    Task<List<Subject>> GetAllWithRelationsAsync(
        string? query = null,
        string? departmentId = null,
        string? academicLevel = null,
        string? electiveType = null,
        bool? hasLab = null,
        bool? isElective = null,
        int page = 1, 
        int pageSize = 50);
    
    Task<int> GetTotalCountAsync(
        string? query = null,
        string? departmentId = null,
        string? academicLevel = null,
        string? electiveType = null,
        bool? hasLab = null,
        bool? isElective = null);
    
    Task<List<Subject>> GetSubjectsByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        string? departmentId = null,
        string? academicLevel = null,
        string? electiveType = null,
        bool? hasLab = null,
        bool? isElective = null,
        int page = 1, 
        int pageSize = 50);
    
    Task<int> GetSubjectsByInstitutionCountAsync(
        Guid institutionId,
        string? query = null,
        string? departmentId = null,
        string? academicLevel = null,
        string? electiveType = null,
        bool? hasLab = null,
        bool? isElective = null);

    // Teacher-related methods
    Task<IEnumerable<Subject>> GetByTeacherIdAsync(Guid teacherId);
    Task<IEnumerable<Subject>> GetByPrimaryTeacherIdAsync(Guid teacherId);
    Task<IEnumerable<Subject>> GetSubjectsByMarksAsync(Guid teacherId);

    // Student-related methods
    Task<IEnumerable<Student>> GetStudentsBySubjectIdAsync(Guid subjectId);

    // Grade-related methods
    Task<IEnumerable<Subject>> GetByGradeIdAsync(Guid gradeId);

    // Elective enrollment methods
    Task<bool> HasEnrolledStudentsAsync(Guid subjectId);
    Task<bool> IsStudentEnrolledAsync(Guid subjectId, Guid studentId);
    Task<int> GetEnrollmentCountAsync(Guid subjectId);
    Task<StudentElective> AddStudentElectiveAsync(StudentElective enrollment);
    Task UpdateStudentElectiveAsync(StudentElective enrollment);
    Task<StudentElective?> GetStudentElectiveAsync(Guid subjectId, Guid studentId);
    Task DeleteStudentElectiveAsync(Guid enrollmentId);
    Task<IEnumerable<StudentElective>> GetStudentElectivesAsync(Guid subjectId);
    Task<IEnumerable<StudentElective>> GetStudentEnrollmentsAsync(Guid studentId);
    Task<bool> UpdateStudentElectiveStatusAsync(Guid subjectId, Guid studentId, ElectiveStatus status);

    // Bulk operations
    Task<IEnumerable<Subject>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task UpdateTeachersAsync(Guid subjectId, IEnumerable<Guid> teacherIds);
    Task UpdateGradesAsync(Guid subjectId, IEnumerable<Guid> gradeIds);

    // Combined operations
    Task<ElectiveEnrollmentValidation> ValidateElectiveEnrollmentAsync(Guid subjectId, Guid studentId);
    Task<ElectiveEnrollmentsData> GetElectiveEnrollmentsWithDetailsAsync(Guid subjectId);
    Task<StudentElectivesData> GetStudentElectivesWithDetailsAsync(Guid studentId);
    Task<SubjectStudentsData> GetStudentsBySubjectWithTeacherValidationAsync(Guid subjectId, Guid teacherId);
    Task<TeacherSubjectsData> GetAllSubjectsByTeacherAsync(Guid teacherId);
    Task<SubjectStudentsByGradeData> GetStudentsBySubjectAndGradeWithTeacherValidationAsync(Guid subjectId, Guid teacherId);
}
