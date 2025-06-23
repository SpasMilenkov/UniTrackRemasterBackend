using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Commons.Repositories;
// Helper class for statistics
public class SemesterStatistics
{
    public Guid SemesterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public SemesterType Type { get; set; }
    public SemesterStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int WeekCount { get; set; }
    public int TotalCourses { get; set; }
    public int TotalStudents { get; set; }
    public bool IsCurrent { get; set; }
    public int DaysRemaining { get; set; }
}
/// <summary>
/// Interface for semester repository operations
/// </summary>
public interface ISemesterRepository : IRepository<Semester>
{
    Task<Semester?> GetByIdAsync(Guid id);
    Task<Semester> CreateAsync(Semester semester);
    Task UpdateAsync(Semester semester);
    Task DeleteAsync(Guid id);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid academicYearId, CancellationToken cancellationToken = default);
    
    // Non-paginated methods
    Task<IEnumerable<Semester>> GetAllAsync();
    Task<IEnumerable<Semester>> GetByAcademicYearAsync(Guid academicYearId);
    Task<IEnumerable<Semester>> GetActiveAsync();
    Task<IEnumerable<Semester>> GetActiveAsync(Guid academicYearId);
    Task<Semester?> GetCurrentAsync(Guid academicYearId);
    Task<Semester?> GetCurrentActiveAsync(Guid institutionId);
    Task<IEnumerable<Semester>> GetByInstitutionAsync(Guid institutionId);
    Task<IEnumerable<Semester>> GetCurrentByInstitutionAsync(Guid institutionId);
    
    // Paginated methods with filtering
    Task<List<Semester>> GetByAcademicYearAsync(Guid academicYearId, string? query = null, string? status = null, string? type = null, bool? isActive = null, int page = 1, int pageSize = 50);
    Task<int> GetByAcademicYearCountAsync(Guid academicYearId, string? query = null, string? status = null, string? type = null, bool? isActive = null);
    
    // Date and status related methods
    Task<Semester?> GetByDateAsync(DateTime date, Guid institutionId);
    Task<Semester?> GetPreviousSemesterAsync(Guid semesterId);
    Task<Semester?> GetNextSemesterAsync(Guid semesterId);
    
    // Validation methods
    Task<bool> IsNameUniqueAsync(string name, Guid academicYearId, Guid? excludeId = null);
    Task<bool> HasDateOverlapAsync(Guid academicYearId, DateTime startDate, DateTime endDate, Guid? excludeId = null);
    Task<bool> HasCoursesAsync(Guid semesterId);
    
    // Statistics and reporting
    Task<SemesterStatistics> GetStatisticsAsync(Guid semesterId);
    
    // Bulk operations
    Task<IEnumerable<Semester>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task UpdateStatusBulkAsync(IEnumerable<Guid> semesterIds, SemesterStatus newStatus);
}