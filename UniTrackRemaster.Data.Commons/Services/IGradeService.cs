using UniTrackRemaster.Api.Dto.Grade;
using UniTrackRemaster.Api.Dto.Student;
using UniTrackRemaster.Api.Dto.Teacher;
using UniTrackRemaster.Commons.Enums;

namespace UniTrackRemaster.Commons.Services;


public interface IGradeService
{
    #region Basic CRUD operations
    Task<GradeResponseDto> GetByIdAsync(Guid id);
    Task<GradeResponseDto> CreateAsync(CreateGradeDto dto);
    Task<GradeResponseDto> UpdateAsync(Guid id, UpdateGradeDto dto);
    Task DeleteAsync(Guid id);
    #endregion

    #region Basic count methods
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    #endregion

    #region Non-paginated methods (for calculations and internal operations)
    Task<IEnumerable<GradeResponseDto>> GetAllAsync();
    Task<IEnumerable<GradeResponseDto>> GetByInstitutionIdAsync(Guid institutionId);
    Task<IEnumerable<GradeResponseDto>> GetByAcademicYearIdAsync(Guid academicYearId);
    Task<IEnumerable<GradeResponseDto>> SearchGradesAsync(string searchTerm);
    #endregion

    #region Paginated methods with filtering (for API endpoints)
    Task<PagedResult<GradeResponseDto>> GetAllAsync(
        string? query = null,
        string? institutionId = null,
        string? academicYearId = null,
        bool? hasHomeRoomTeacher = null,
        int page = 1, 
        int pageSize = 50);

    Task<PagedResult<GradeResponseDto>> GetGradesByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        string? academicYearId = null,
        bool? hasHomeRoomTeacher = null,
        int page = 1, 
        int pageSize = 50);
    #endregion

    #region Grade-specific functionality
    Task<IEnumerable<TeacherResponseDto>> GetAssignedTeachersAsync(Guid gradeId);
    Task<IEnumerable<StudentResponseDto>> GetGradeStudentsAsync(Guid gradeId);
    Task AssignTeacherAsync(Guid gradeId, Guid teacherId);
    Task RemoveTeacherAsync(Guid gradeId, Guid teacherId);
    #endregion

    #region Access control
    Task<bool> CanAccessGrade(Guid gradeId, Guid userId, Guid institutionId, Roles role);
    #endregion
}
