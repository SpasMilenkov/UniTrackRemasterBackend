using UniTrackRemaster.Api.Dto.Department;
using UniTrackRemaster.Api.Dto.Faculty;
using UniTrackRemaster.Api.Dto.Major;
using UniTrackRemaster.Commons.Enums;

namespace UniTrackRemaster.Commons.Services;

public interface IFacultyService
{
    #region Basic CRUD operations
    Task<FacultyResponseDto> GetByIdAsync(Guid id);
    Task<FacultyResponseDto> CreateAsync(CreateFacultyDto dto);
    Task<FacultyResponseDto> UpdateAsync(Guid id, UpdateFacultyDto dto);
    Task DeleteAsync(Guid id);
    #endregion

    #region Basic count methods
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    #endregion

    #region Non-paginated methods (for calculations and internal operations)
    Task<IEnumerable<FacultyResponseDto>> GetAllAsync();
    Task<IEnumerable<FacultyResponseDto>> GetByUniversityAsync(Guid universityId);
    Task<IEnumerable<FacultyResponseDto>> GetByInstitutionIdAsync(Guid institutionId);
    Task<IEnumerable<FacultyResponseDto>> SearchFacultiesAsync(string searchTerm);
    #endregion

    #region Paginated methods with filtering (for API endpoints)
    Task<PagedResult<FacultyResponseDto>> GetAllAsync(
        string? query = null,
        string? universityId = null,
        string? institutionId = null,
        string? status = null,
        int page = 1, 
        int pageSize = 50);

    Task<PagedResult<FacultyResponseDto>> GetFacultiesByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        string? universityId = null,
        string? status = null,
        int page = 1, 
        int pageSize = 50);
    #endregion

    #region Faculty-specific functionality
    Task<IEnumerable<DepartmentResponseDto>> GetFacultyDepartmentsAsync(Guid facultyId);
    Task<IEnumerable<MajorResponseDto>> GetFacultyMajorsAsync(Guid facultyId);
    #endregion

    #region Access control
    Task<bool> CanAccessFaculty(Guid facultyId, Guid userId, Guid institutionId, Roles role);
    #endregion
}
