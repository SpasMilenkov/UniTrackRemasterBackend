using UniTrackRemaster.Api.Dto.Semester;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Commons.Services;

public interface ISemesterService
{
    #region Basic CRUD operations

    Task<SemesterResponseDto> GetByIdAsync(Guid id);
    Task<SemesterResponseDto> CreateAsync(CreateSemesterDto dto);
    Task<SemesterResponseDto> UpdateAsync(Guid id, UpdateSemesterDto dto);
    Task DeleteAsync(Guid id);

    #endregion

    #region Paginated methods with filtering (Academic Year Scoped)

    Task<PagedResult<SemesterResponseDto>> GetByAcademicYearAsync(
        Guid academicYearId,
        string? query = null,
        string? status = null,
        string? type = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 50);

    #endregion

    #region Non-paginated methods (for dropdowns, calculations)

    Task<IEnumerable<SemesterResponseDto>> GetAllByAcademicYearAsync(Guid academicYearId);
    Task<SemesterResponseDto> GetCurrentAsync(Guid academicYearId);
    Task<IEnumerable<SemesterResponseDto>> GetActiveAsync(Guid academicYearId);

    #endregion

    #region Global methods (Use Sparingly - Security Risk)

    Task<IEnumerable<SemesterResponseDto>> GetAllAsync();
    Task<IEnumerable<SemesterResponseDto>> GetAllActiveAsync();
    Task<IEnumerable<SemesterResponseDto>> GetByInstitutionAsync(Guid institutionId);
    Task<IEnumerable<SemesterResponseDto>> GetCurrentByInstitutionAsync(Guid institutionId);

    #endregion

    #region Legacy methods (for backward compatibility)

    Task<IEnumerable<SemesterResponseDto>> GetByAcademicYearAsync(Guid academicYearId);
    Task<IEnumerable<SemesterResponseDto>> GetActiveAsync();

    #endregion

    #region Statistics and reporting

    Task<SemesterStatistics> GetStatisticsAsync(Guid id);
    Task<int> GetTotalCountAsync(Guid academicYearId);

    #endregion

    #region Status management

    Task SetStatusAsync(Guid id, SemesterStatus status);

    #endregion
}