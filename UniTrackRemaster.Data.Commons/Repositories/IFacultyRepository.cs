using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Commons.Repositories;


// Supporting classes for complex operations
public class FacultyDepartmentsData
{
    public bool FacultyNotFound { get; set; }
    public string FacultyName { get; set; } = string.Empty;
    public List<Department> Departments { get; set; } = new();
}

public class FacultyMajorsData
{
    public bool FacultyNotFound { get; set; }
    public string FacultyName { get; set; } = string.Empty;
    public List<Major> Majors { get; set; } = new();
}

public interface IFacultyRepository : IRepository<Faculty>
{
    #region Basic CRUD operations
    Task<Faculty?> GetByIdAsync(Guid id);
    Task<Faculty?> GetByIdWithRelationsAsync(Guid id);
    Task<Faculty> CreateAsync(Faculty faculty);
    Task UpdateAsync(Faculty faculty);
    Task DeleteAsync(Guid id);
    #endregion

    #region Basic count and exists methods
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid universityId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, Guid universityId, CancellationToken cancellationToken = default);
    Task<bool> HasDepartmentsAsync(Guid facultyId, CancellationToken cancellationToken = default);
    Task<bool> HasMajorsAsync(Guid facultyId, CancellationToken cancellationToken = default);
    #endregion

    #region Non-paginated methods (for calculations and internal operations)
    Task<IEnumerable<Faculty>> GetAllAsync();
    Task<IEnumerable<Faculty>> GetAllWithRelationsAsync();
    Task<IEnumerable<Faculty>> GetByUniversityAsync(Guid universityId);
    Task<IEnumerable<Faculty>> GetByUniversityWithRelationsAsync(Guid universityId);
    Task<IEnumerable<Faculty>> GetByInstitutionAsync(Guid institutionId);
    Task<IEnumerable<Faculty>> GetByInstitutionWithRelationsAsync(Guid institutionId);
    Task<IEnumerable<Faculty>> SearchAsync(string searchTerm);
    Task<IEnumerable<Faculty>> SearchWithRelationsAsync(string searchTerm);
    #endregion

    #region Paginated methods with filtering (for API endpoints)
    Task<List<Faculty>> GetAllWithRelationsAsync(
        string? query = null,
        string? universityId = null,
        string? institutionId = null,
        string? status = null,
        int page = 1, 
        int pageSize = 50);

    Task<int> GetTotalCountAsync(
        string? query = null,
        string? universityId = null,
        string? institutionId = null,
        string? status = null);

    Task<List<Faculty>> GetFacultiesByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        string? universityId = null,
        string? status = null,
        int page = 1, 
        int pageSize = 50);

    Task<int> GetFacultiesByInstitutionCountAsync(
        Guid institutionId,
        string? query = null,
        string? universityId = null,
        string? status = null);
    #endregion

    #region Validation and lookup methods
    Task<Faculty?> GetByNameAndUniversityAsync(string name, Guid universityId);
    Task<Faculty?> GetByCodeAndUniversityAsync(string code, Guid universityId);
    #endregion

    #region Department-related methods
    Task<IEnumerable<Department>> GetDepartmentsByFacultyIdAsync(Guid facultyId);
    #endregion

    #region Major-related methods
    Task<IEnumerable<Major>> GetMajorsByFacultyIdAsync(Guid facultyId);
    #endregion

    #region Bulk operations
    Task<IEnumerable<Faculty>> GetByIdsAsync(IEnumerable<Guid> ids);
    #endregion

    #region Combined operations
    Task<FacultyDepartmentsData> GetFacultyDepartmentsWithValidationAsync(Guid facultyId);
    Task<FacultyMajorsData> GetFacultyMajorsWithValidationAsync(Guid facultyId);
    #endregion
}