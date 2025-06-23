using UniTrackRemaster.Api.Dto.AcademicYear;
using UniTrackRemaster.Commons.Repositories;

namespace UniTrackRemaster.Commons.Services;

public interface IAcademicYearService
{
    #region Basic CRUD operations

    Task<AcademicYearResponseDto> GetByIdAsync(Guid id);
    Task<AcademicYearResponseDto> CreateAsync(CreateAcademicYearDto dto);
    Task<AcademicYearResponseDto> UpdateAsync(Guid id, UpdateAcademicYearDto dto);
    Task DeleteAsync(Guid id);

    #endregion

    #region Paginated methods with filtering

    Task<PagedResult<AcademicYearResponseDto>> GetByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        bool? isActive = null,
        bool? isCurrent = null,
        int page = 1,
        int pageSize = 50);

    #endregion

    #region Non-paginated methods (for dropdowns, calculations)

    Task<IEnumerable<AcademicYearResponseDto>> GetAllByInstitutionAsync(Guid institutionId);
    Task<IEnumerable<AcademicYearResponseDto>> GetByInstitutionAsync(Guid institutionId);
    Task<AcademicYearResponseDto> GetCurrentAsync(Guid institutionId);
    Task<IEnumerable<AcademicYearResponseDto>> GetActiveAsync(Guid institutionId);

    #endregion

    #region Statistics and reporting

    Task<AcademicYearStatistics> GetStatisticsAsync(Guid id);
    Task<int> GetTotalCountAsync(Guid institutionId);

    #endregion
}