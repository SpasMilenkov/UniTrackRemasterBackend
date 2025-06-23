using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Commons.Repositories;

// Supporting classes for complex operations

public class DepartmentTeachersData
{
    public bool DepartmentNotFound { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public List<Teacher> Teachers { get; set; } = new();
}
public interface IDepartmentRepository : IRepository<Department>
{
    #region Basic CRUD operations
    Task<Department?> GetByIdAsync(Guid id);
    Task<Department?> GetByIdWithRelationsAsync(Guid id);
    Task<Department> CreateAsync(Department department);
    Task UpdateAsync(Department department);
    Task DeleteAsync(Guid id);
    #endregion

    #region Basic count and exists methods
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid facultyId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, Guid facultyId, CancellationToken cancellationToken = default);
    Task<bool> HasTeachersAsync(Guid departmentId, CancellationToken cancellationToken = default);
    #endregion

    #region Non-paginated methods (for calculations and internal operations)
    Task<IEnumerable<Department>> GetAllAsync();
    Task<IEnumerable<Department>> GetAllWithRelationsAsync();
    Task<IEnumerable<Department>> GetByFacultyAsync(Guid facultyId);
    Task<IEnumerable<Department>> GetByFacultyWithRelationsAsync(Guid facultyId);
    Task<IEnumerable<Department>> GetByInstitutionAsync(Guid institutionId);
    Task<IEnumerable<Department>> GetByInstitutionWithRelationsAsync(Guid institutionId);
    Task<IEnumerable<Department>> SearchAsync(string searchTerm);
    Task<IEnumerable<Department>> SearchWithRelationsAsync(string searchTerm);
    #endregion

    #region Paginated methods with filtering (for API endpoints)
    Task<List<Department>> GetAllWithRelationsAsync(
        string? query = null,
        string? facultyId = null,
        string? institutionId = null,
        string? type = null,
        string? status = null,
        int page = 1, 
        int pageSize = 50);

    Task<int> GetTotalCountAsync(
        string? query = null,
        string? facultyId = null,
        string? institutionId = null,
        string? type = null,
        string? status = null);

    Task<List<Department>> GetDepartmentsByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        string? facultyId = null,
        string? type = null,
        string? status = null,
        int page = 1, 
        int pageSize = 50);

    Task<int> GetDepartmentsByInstitutionCountAsync(
        Guid institutionId,
        string? query = null,
        string? facultyId = null,
        string? type = null,
        string? status = null);
    #endregion

    #region Validation and lookup methods
    Task<Department?> GetByNameAndFacultyAsync(string name, Guid facultyId);
    Task<Department?> GetByCodeAndFacultyAsync(string code, Guid facultyId);
    #endregion

    #region Teacher-related methods
    Task<IEnumerable<Teacher>> GetTeachersByDepartmentIdAsync(Guid departmentId);
    Task AssignTeacherAsync(Guid departmentId, Guid teacherId);
    Task<bool> RemoveTeacherAsync(Guid departmentId, Guid teacherId);
    #endregion

    #region Bulk operations
    Task<IEnumerable<Department>> GetByIdsAsync(IEnumerable<Guid> ids);
    #endregion

    #region Combined operations
    Task<TeacherAssignmentValidation> ValidateTeacherAssignmentAsync(Guid departmentId, Guid teacherId);
    Task<DepartmentTeachersData> GetDepartmentTeachersWithValidationAsync(Guid departmentId);
    #endregion
}