using UniTrackRemaster.Api.Dto.Student;
using UniTrackRemaster.Api.Dto.Subject;
using UniTrackRemaster.Commons.Enums;

namespace UniTrackRemaster.Commons.Services;

public interface ISubjectService
{
    // Basic CRUD operations
    Task<SubjectResponseDto> GetByIdAsync(Guid id);
    Task<SubjectResponseDto> CreateAsync(CreateSubjectDto dto);
    Task<SubjectResponseDto> UpdateAsync(Guid id, UpdateSubjectDto dto);
    Task DeleteAsync(Guid id);
    
    // Basic count methods
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    // Non-paginated methods (for calculations and internal operations)
    Task<IEnumerable<SubjectResponseDto>> GetAllAsync();
    Task<IEnumerable<SubjectResponseDto>> GetByDepartmentAsync(Guid departmentId);
    Task<IEnumerable<SubjectResponseDto>> GetElectivesAsync(bool activeOnly = true);
    Task<IEnumerable<SubjectResponseDto>> SearchSubjectsAsync(string searchTerm);

    // Paginated methods with filtering (for API endpoints)
    Task<PagedResult<SubjectResponseDto>> GetAllAsync(
        string? query = null,
        string? departmentId = null,
        string? academicLevel = null,
        string? electiveType = null,
        bool? hasLab = null,
        bool? isElective = null,
        int page = 1, 
        int pageSize = 50);
    
    Task<PagedResult<SubjectResponseDto>> GetSubjectsByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        string? departmentId = null,
        string? academicLevel = null,
        string? electiveType = null,
        bool? hasLab = null,
        bool? isElective = null,
        int page = 1, 
        int pageSize = 50);

    // Elective management
    Task EnrollStudentInElectiveAsync(Guid subjectId, Guid studentId);
    Task UnenrollStudentFromElectiveAsync(Guid subjectId, Guid studentId);
    Task<IEnumerable<StudentElectiveResponseDto>> GetElectiveEnrollmentsAsync(Guid subjectId);
    Task<IEnumerable<SubjectResponseDto>> GetStudentElectivesAsync(Guid studentId);

    // Teacher and student access methods
    Task<IEnumerable<StudentResponseDto>> GetStudentsBySubjectAsync(Guid subjectId, Guid teacherId);
    Task<IEnumerable<SubjectResponseDto>> GetSubjectsByTeacherAsync(Guid teacherId);
    Task<IEnumerable<StudentsByGradeDto>> GetStudentsBySubjectAndGradeAsync(Guid subjectId, Guid teacherId);

    // Access control
    Task<bool> CanAccessSubject(Guid subjectId, Guid userId, Guid institutionId, Roles role);
}
