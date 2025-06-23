using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Commons.Repositories;

/// <summary>
/// Student repository interface with semester-aware analytics methods
/// </summary>
public interface IStudentRepository : IRepository<Student>
{
    #region Basic CRUD Operations

    /// <summary>
    /// Gets a student by ID with all related data
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <returns>Student with related data or null if not found</returns>
    Task<Student?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a student by user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Student associated with the user or null if not found</returns>
    Task<Student?> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets students by school
    /// </summary>
    /// <param name="schoolId">School ID</param>
    /// <returns>List of students in the school</returns>
    Task<IEnumerable<Student>> GetBySchoolAsync(Guid schoolId);

    /// <summary>
    /// Gets students by university
    /// </summary>
    /// <param name="universityId">University ID</param>
    /// <returns>List of students in the university</returns>
    Task<IEnumerable<Student>> GetByUniversityAsync(Guid universityId);

    /// <summary>
    /// Gets students by grade
    /// </summary>
    /// <param name="gradeId">Grade ID</param>
    /// <returns>List of students in the grade</returns>
    Task<IEnumerable<Student>> GetByGradeIdAsync(Guid gradeId);

    /// <summary>
    /// Creates a new student
    /// </summary>
    /// <param name="student">Student to create</param>
    /// <returns>Created student</returns>
    Task<Student> CreateAsync(Student student);

    /// <summary>
    /// Updates an existing student
    /// </summary>
    /// <param name="student">Student to update</param>
    Task UpdateAsync(Student student);

    /// <summary>
    /// Deletes a student by ID
    /// </summary>
    /// <param name="id">Student ID to delete</param>
    Task DeleteAsync(Guid id);

    #endregion

    #region Search and Pagination

    /// <summary>
    /// Searches students with pagination and filtering
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="gradeId">Grade filter</param>
    /// <param name="institutionId">Institution filter</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="sortBy">Sort field</param>
    /// <param name="ascending">Sort direction</param>
    /// <returns>Paginated students result</returns>
    Task<(IEnumerable<Student> Students, int TotalCount)> SearchWithPaginationAsync(
        string? query = null,
        Guid? gradeId = null,
        Guid? institutionId = null,
        int pageNumber = 1,
        int pageSize = 20,
        string sortBy = "FirstName",
        bool ascending = true);

    #endregion

    #region Invitation System

    /// <summary>
    /// Gets pending students for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of pending student profiles</returns>
    Task<IEnumerable<Student>> GetPendingByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets students by institution with optional status filter
    /// </summary>
    /// <param name="institutionId">Institution ID</param>
    /// <param name="status">Optional status filter</param>
    /// <returns>List of students</returns>
    Task<IEnumerable<Student>> GetByInstitutionAsync(Guid institutionId, ProfileStatus? status = null);

    /// <summary>
    /// Gets students by status
    /// </summary>
    /// <param name="status">Profile status</param>
    /// <returns>List of students with the specified status</returns>
    Task<IEnumerable<Student>> GetByStatusAsync(ProfileStatus status);

    #endregion

    #region Semester-Aware Analytics Methods

    /// <summary>
    /// Gets comprehensive dashboard data for a student with semester awareness
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="academicYear">Optional academic year filter</param>
    /// <param name="semester">Optional semester filter</param>
    /// <returns>Dashboard data including marks, subjects, teachers, classmates, and semesters</returns>
    Task<(
        List<Mark> Marks,
        Dictionary<Guid, Subject> Subjects,
        Dictionary<Guid, Teacher> Teachers,
        List<Student> Classmates,
        Dictionary<Guid, Semester> Semesters
        )> GetStudentDashboardDataAsync(Guid studentId, AcademicYear? academicYear = null, Semester? semester = null);

    /// <summary>
    /// Gets student marks with related subjects and semester information
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="academicYear">Optional academic year filter</param>
    /// <param name="semester">Optional semester filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Marks, subjects, and semester data</returns>
    Task<(List<Mark> Marks, Dictionary<Guid, Subject> Subjects, Dictionary<Guid, Semester> Semesters)>
        GetStudentMarksWithSubjectsAsync(
            Guid studentId, AcademicYear? academicYear = null, Semester? semester = null,
            CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets student marks grouped by semesters for transcript generation
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="institutionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of semester names to marks and semester data</returns>
    Task<Dictionary<string, (List<Mark> Marks, Semester Semester)>> GetStudentMarksBySemestersAsync(
        Guid studentId, Guid institutionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets student attendance data with semester awareness
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="academicYear">Optional academic year filter</param>
    /// <param name="semester">Optional semester filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of attendance records</returns>
    Task<List<Absence>> GetStudentAttendanceDataAsync(
        Guid studentId, AcademicYear? academicYear = null, Semester? semester = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets class performance data for comparison with semester awareness
    /// </summary>
    /// <param name="gradeId">Grade/class ID</param>
    /// <param name="academicYear">Optional academic year filter</param>
    /// <param name="semester">Optional semester filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Performance data grouped by subject and student</returns>
    Task<Dictionary<Guid, List<(Guid StudentId, List<Mark> Marks)>>> GetClassPerformanceDataAsync(
        Guid gradeId, AcademicYear? academicYear = null, Semester? semester = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets detailed breakdown of academic year by semesters
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="academicYearId">Academic year ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Subject breakdown by semester</returns>
    Task<Dictionary<Guid, List<(Semester Semester, List<Mark> Marks)>>> GetAcademicYearBreakdownAsync(
        Guid studentId, Guid academicYearId, CancellationToken cancellationToken = default);

    #endregion
}