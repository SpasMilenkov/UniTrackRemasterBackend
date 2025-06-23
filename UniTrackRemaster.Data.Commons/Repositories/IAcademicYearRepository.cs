using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Commons.Repositories;
public class AcademicYearStatistics
{
    public Guid AcademicYearId { get; set; }
    public int TotalSemesters { get; set; }
    public int ActiveSemesters { get; set; }
    public int TotalStudents { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrent { get; set; }
}
public interface IAcademicYearRepository : IRepository<AcademicYear>
{
    #region Basic CRUD operations

    Task<AcademicYear?> GetByIdAsync(Guid id);
    Task<AcademicYear> CreateAsync(AcademicYear academicYear);
    Task UpdateAsync(AcademicYear academicYear);
    Task DeleteAsync(Guid id);

    #endregion

    #region Basic count and exists methods

    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid institutionId, CancellationToken cancellationToken = default);

    #endregion

    #region Non-paginated methods (for calculations and internal operations)

    Task<IEnumerable<AcademicYear>> GetAllAsync(Guid institutionId);
    Task<IEnumerable<AcademicYear>> GetByInstitutionAsync(Guid institutionId);
    Task<AcademicYear?> GetCurrentAsync(Guid institutionId);
    Task<IEnumerable<AcademicYear>> GetActiveAsync(Guid institutionId);

    #endregion

    #region Paginated methods with filtering (for API endpoints)

    Task<List<AcademicYear>> GetByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        bool? isActive = null,
        bool? isCurrent = null,
        int page = 1,
        int pageSize = 50);

    Task<int> GetByInstitutionCountAsync(
        Guid institutionId,
        string? query = null,
        bool? isActive = null,
        bool? isCurrent = null);

    #endregion

    #region Semester-related methods

    Task<bool> HasSemestersAsync(Guid academicYearId);
    Task<int> GetSemesterCountAsync(Guid academicYearId);

    #endregion

    #region Validation methods

    Task<bool> IsNameUniqueAsync(string name, Guid institutionId, Guid? excludeId = null);
    Task<bool> HasDateOverlapAsync(Guid institutionId, DateTime startDate, DateTime endDate, Guid? excludeId = null);

    #endregion

    #region Statistics and reporting

    Task<AcademicYearStatistics> GetStatisticsAsync(Guid academicYearId);

    #endregion
}