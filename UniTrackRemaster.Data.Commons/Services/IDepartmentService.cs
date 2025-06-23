using UniTrackRemaster.Api.Dto.Department;
using UniTrackRemaster.Api.Dto.Teacher;
using UniTrackRemaster.Commons.Enums;

namespace UniTrackRemaster.Commons.Services;

public interface IDepartmentService
{
    #region Basic CRUD operations
    Task<DepartmentResponseDto> GetByIdAsync(Guid id);
    Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto);
    Task<DepartmentResponseDto> UpdateAsync(Guid id, UpdateDepartmentDto dto);
    Task DeleteAsync(Guid id);
    #endregion

    #region Basic count methods
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    #endregion

    #region Non-paginated methods (for calculations and internal operations)
    Task<IEnumerable<DepartmentResponseDto>> GetAllAsync();
    Task<IEnumerable<DepartmentResponseDto>> GetByFacultyAsync(Guid facultyId);
    Task<IEnumerable<DepartmentResponseDto>> GetByInstitutionIdAsync(Guid institutionId);
    Task<IEnumerable<DepartmentResponseDto>> SearchDepartmentsAsync(string searchTerm);
    #endregion

    #region Paginated methods with filtering (for API endpoints)
    Task<PagedResult<DepartmentResponseDto>> GetAllAsync(
        string? query = null,
        string? facultyId = null,
        string? institutionId = null,
        string? type = null,
        string? status = null,
        int page = 1, 
        int pageSize = 50);

    Task<PagedResult<DepartmentResponseDto>> GetDepartmentsByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        string? facultyId = null,
        string? type = null,
        string? status = null,
        int page = 1, 
        int pageSize = 50);
    #endregion

    #region Department-specific functionality
    Task<IEnumerable<TeacherResponseDto>> GetDepartmentTeachersAsync(Guid departmentId);
    Task AssignTeacherAsync(Guid departmentId, Guid teacherId);
    Task RemoveTeacherAsync(Guid departmentId, Guid teacherId);
    #endregion

    #region Access control
    Task<bool> CanAccessDepartment(Guid departmentId, Guid userId, Guid institutionId, Roles role);
    #endregion
}