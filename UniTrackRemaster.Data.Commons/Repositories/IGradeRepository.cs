using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Commons.Repositories;

// Supporting classes for complex operations
public class TeacherAssignmentValidation
{
    public bool GradeNotFound { get; set; }
    public bool TeacherNotFound { get; set; }
    public bool AlreadyAssigned { get; set; }
}

public class GradeTeachersData
{
    public bool GradeNotFound { get; set; }
    public string GradeName { get; set; } = string.Empty;
    public List<Teacher> Teachers { get; set; } = new();
    public Teacher? HomeRoomTeacher { get; set; }
}

public class GradeStudentsData
{
    public bool GradeNotFound { get; set; }
    public string GradeName { get; set; } = string.Empty;
    public List<Student> Students { get; set; } = new();
}


public interface IGradeRepository : IRepository<Grade>
{
    #region Basic CRUD operations
    Task<Grade?> GetByIdAsync(Guid id);
    Task<Grade?> GetByIdWithRelationsAsync(Guid id);
    Task<Grade> CreateAsync(Grade grade);
    Task UpdateAsync(Grade grade);
    Task DeleteAsync(Guid id);
    #endregion

    #region Basic count and exists methods
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid institutionId, CancellationToken cancellationToken = default);
    Task<bool> HasStudentsAsync(Guid gradeId, CancellationToken cancellationToken = default);
    #endregion

    #region Non-paginated methods (for calculations and internal operations)
    Task<IEnumerable<Grade>> GetAllAsync();
    Task<IEnumerable<Grade>> GetAllWithRelationsAsync();
    Task<IEnumerable<Grade>> GetByInstitutionAsync(Guid institutionId);
    Task<IEnumerable<Grade>> GetByInstitutionWithRelationsAsync(Guid institutionId);
    Task<IEnumerable<Grade>> GetByAcademicYearAsync(Guid academicYearId);
    Task<IEnumerable<Grade>> GetByAcademicYearWithRelationsAsync(Guid academicYearId);
    Task<IEnumerable<Grade>> SearchAsync(string searchTerm);
    Task<IEnumerable<Grade>> SearchWithRelationsAsync(string searchTerm);
    #endregion

    #region Paginated methods with filtering (for API endpoints)
    Task<List<Grade>> GetAllWithRelationsAsync(
        string? query = null,
        string? institutionId = null,
        string? academicYearId = null,
        bool? hasHomeRoomTeacher = null,
        int page = 1, 
        int pageSize = 50);

    Task<int> GetTotalCountAsync(
        string? query = null,
        string? institutionId = null,
        string? academicYearId = null,
        bool? hasHomeRoomTeacher = null);

    Task<List<Grade>> GetGradesByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        string? academicYearId = null,
        bool? hasHomeRoomTeacher = null,
        int page = 1, 
        int pageSize = 50);

    Task<int> GetGradesByInstitutionCountAsync(
        Guid institutionId,
        string? query = null,
        string? academicYearId = null,
        bool? hasHomeRoomTeacher = null);
    #endregion

    #region Teacher-related methods
    Task<IEnumerable<Grade>> GetByTeacherIdAsync(Guid teacherId);
    Task<IEnumerable<Grade>> GetByHomeRoomTeacherIdAsync(Guid teacherId);
    Task AssignTeacherAsync(Guid gradeId, Guid teacherId);
    Task<bool> RemoveTeacherAsync(Guid gradeId, Guid teacherId);
    #endregion

    #region Student-related methods
    Task<IEnumerable<Student>> GetStudentsByGradeIdAsync(Guid gradeId);
    #endregion

    #region Bulk operations
    Task<IEnumerable<Grade>> GetByIdsAsync(IEnumerable<Guid> ids);
    #endregion

    #region Combined operations
    Task<TeacherAssignmentValidation> ValidateTeacherAssignmentAsync(Guid gradeId, Guid teacherId);
    Task<GradeTeachersData> GetGradeTeachersWithValidationAsync(Guid gradeId);
    Task<GradeStudentsData> GetGradeStudentsWithValidationAsync(Guid gradeId);
    #endregion
}