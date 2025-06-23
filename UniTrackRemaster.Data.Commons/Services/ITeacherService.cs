using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Api.Dto.Student;
using UniTrackRemaster.Api.Dto.Teacher;
using UniTrackRemaster.Api.Dto.Teacher.Analytics;

namespace UniTrackRemaster.Commons.Services;

/// <summary>
/// Interface for teacher service operations with semester support and grading system integration.
/// Provides comprehensive teacher management, academic activities tracking, and advanced analytics.
/// </summary>
public interface ITeacherService
{
    #region Basic CRUD Operations

    /// <summary>
    /// Retrieves a teacher by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the teacher</param>
    /// <returns>The teacher information</returns>
    /// <exception cref="NotFoundException">Thrown when the teacher is not found</exception>
    Task<TeacherResponseDto> GetByIdAsync(Guid id);

    /// <summary>
    /// Retrieves all teachers in the system.
    /// </summary>
    /// <returns>A collection of all teachers</returns>
    Task<IEnumerable<TeacherResponseDto>> GetAllAsync();

    /// <summary>
    /// Retrieves all teachers for a specific institution.
    /// </summary>
    /// <param name="institutionId">The unique identifier of the institution</param>
    /// <returns>A collection of teachers in the institution</returns>
    Task<IEnumerable<TeacherResponseDto>> GetByInstitutionIdAsync(Guid institutionId);

    /// <summary>
    /// Retrieves a teacher by their associated user ID.
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>The teacher information if found, null otherwise</returns>
    Task<TeacherResponseDto?> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Creates a new teacher invitation with pending status.
    /// </summary>
    /// <param name="dto">The teacher creation data</param>
    /// <returns>The created teacher information</returns>
    /// <exception cref="ValidationException">Thrown when required data is missing or invalid</exception>
    /// <exception cref="NotFoundException">Thrown when referenced entities are not found</exception>
    /// <exception cref="InvalidOperationException">Thrown when user already has a teacher profile</exception>
    Task<TeacherResponseDto> CreateAsync(CreateTeacherDto dto);

    /// <summary>
    /// Updates an existing teacher's information.
    /// </summary>
    /// <param name="id">The unique identifier of the teacher</param>
    /// <param name="dto">The updated teacher information</param>
    /// <returns>The updated teacher information</returns>
    /// <exception cref="NotFoundException">Thrown when the teacher is not found</exception>
    Task<TeacherResponseDto> UpdateAsync(Guid id, UpdateTeacherDto dto);

    /// <summary>
    /// Deletes a teacher from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the teacher</param>
    /// <exception cref="NotFoundException">Thrown when the teacher is not found</exception>
    Task DeleteAsync(Guid id);

    #endregion

    #region Search and Filtering

    /// <summary>
    /// Searches for teachers based on specified criteria with pagination support.
    /// </summary>
    /// <param name="searchParams">The search parameters including filters and pagination</param>
    /// <returns>A paginated response containing matching teachers and metadata</returns>
    /// <exception cref="ApplicationException">Thrown when an error occurs during search</exception>
    Task<TeacherSearchResponse> SearchTeachersAsync(TeacherSearchParams searchParams);

    #endregion

    #region Subject and Student Management

    /// <summary>
    /// Retrieves students in a specific subject taught by the teacher, grouped by grade.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="subjectId">The unique identifier of the subject</param>
    /// <returns>Students grouped by grade for the specified subject</returns>
    /// <exception cref="NotFoundException">Thrown when the teacher is not found</exception>
    Task<IEnumerable<StudentsByGradeDto>> GetStudentsBySubjectAndGradeAsync(Guid teacherId, Guid subjectId);

    #endregion

    #region Dashboard and Analytics

    /// <summary>
    /// Retrieves comprehensive teacher dashboard data with semester context and grading system integration.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="semesterId">Optional semester ID for filtering (defaults to current active semester)</param>
    /// <returns>Dashboard data including subjects, students, recent activities, and statistics</returns>
    /// <exception cref="NotFoundException">Thrown when the teacher or semester is not found</exception>
    Task<TeacherDashboardDto> GetTeacherDashboardAsync(Guid teacherId, Guid? semesterId = null);

    /// <summary>
    /// Retrieves attendance overview data for dashboard visualization with semester filtering.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="filterParams">Filter parameters including semester, date range, and entity filters</param>
    /// <returns>Attendance overview data with trend analysis and semester context</returns>
    /// <exception cref="NotFoundException">Thrown when the teacher or semester is not found</exception>
    Task<AttendanceOverviewDto> GetAttendanceOverviewAsync(Guid teacherId, AttendanceFilterParams filterParams);

    /// <summary>
    /// Retrieves excused vs unexcused absence breakdown data for analysis with semester filtering.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="filterParams">Filter parameters including semester, date range, and entity filters</param>
    /// <returns>Excused vs unexcused absence breakdown with subject-level details and semester context</returns>
    /// <exception cref="NotFoundException">Thrown when the teacher or semester is not found</exception>
    Task<ExcusedUnexcusedBreakdownDto> GetExcusedUnexcusedBreakdownAsync(Guid teacherId, AbsenceBreakdownFilterParams filterParams);

    /// <summary>
    /// Retrieves a list of at-risk students based on semester-specific absence patterns.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="filterParams">Filter parameters including risk thresholds, semester, and entity filters</param>
    /// <returns>List of at-risk students with absence statistics and risk analysis</returns>
    /// <exception cref="NotFoundException">Thrown when the teacher or semester is not found</exception>
    Task<AtRiskStudentsResponseDto> GetAtRiskStudentsAsync(Guid teacherId, AtRiskStudentsFilterParams filterParams);

    /// <summary>
    /// Retrieves comprehensive attendance statistics with semester filtering for data visualization and analytics.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="filterParams">Filter parameters including semester, date range, and entity filters</param>
    /// <returns>Comprehensive attendance statistics with trends and semester context</returns>
    /// <exception cref="NotFoundException">Thrown when the teacher or semester is not found</exception>
    Task<AttendanceStatisticsDto> GetAttendanceStatisticsAsync(Guid teacherId, StatisticsFilterParams filterParams);

    #endregion

    #region Grade Assignment Management

    /// <summary>
    /// Assigns a teacher to teach multiple grades directly.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="dto">The grade assignment request containing grade IDs</param>
    /// <returns>Assignment result with success details and impact metrics</returns>
    /// <exception cref="NotFoundException">Thrown when the teacher or grades are not found</exception>
    /// <exception cref="ValidationException">Thrown when validation fails</exception>
    /// <exception cref="ApplicationException">Thrown when an error occurs during assignment</exception>
    Task<TeacherGradeAssignmentResultDto> AssignTeacherToGradesAsync(Guid teacherId, AssignTeacherToGradesDto dto);

    /// <summary>
    /// Removes a teacher's assignment from specific grades.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="dto">The grade unassignment request containing grade IDs</param>
    /// <returns>Unassignment result with success details and impact metrics</returns>
    /// <exception cref="NotFoundException">Thrown when the teacher is not found</exception>
    /// <exception cref="ValidationException">Thrown when teacher is not assigned to specified grades</exception>
    /// <exception cref="ApplicationException">Thrown when an error occurs during unassignment</exception>
    Task<TeacherGradeAssignmentResultDto> UnassignTeacherFromGradesAsync(Guid teacherId, UnassignTeacherFromGradesDto dto);

    /// <summary>
    /// Updates all grade assignments for a teacher in one operation.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="dto">The grade assignment update request containing new grade IDs</param>
    /// <returns>Update result with success details and impact metrics</returns>
    /// <exception cref="NotFoundException">Thrown when the teacher or grades are not found</exception>
    /// <exception cref="ValidationException">Thrown when validation fails</exception>
    /// <exception cref="ApplicationException">Thrown when an error occurs during update</exception>
    Task<TeacherGradeAssignmentResultDto> UpdateTeacherGradeAssignmentsAsync(Guid teacherId, UpdateTeacherGradeAssignmentsDto dto);

    /// <summary>
    /// Retrieves a teacher with all their grade assignments and related information.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <returns>Detailed teacher information with complete grade assignment details</returns>
    /// <exception cref="NotFoundException">Thrown when the teacher is not found</exception>
    Task<TeacherWithGradeAssignmentsResponseDto> GetTeacherWithGradeAssignmentsAsync(Guid teacherId);

    /// <summary>
    /// Retrieves all grades assigned to a specific teacher.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <returns>List of grade assignments with details and metadata</returns>
    /// <exception cref="NotFoundException">Thrown when the teacher is not found</exception>
    Task<IEnumerable<GradeAssignmentResponseDto>> GetTeacherAssignedGradesAsync(Guid teacherId);

    /// <summary>
    /// Retrieves a comprehensive summary of a teacher's grade assignments.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <returns>Summary of teacher's grade assignments with counts and breakdown</returns>
    /// <exception cref="NotFoundException">Thrown when the teacher is not found</exception>
    Task<TeacherGradeAssignmentSummaryDto> GetTeacherGradeAssignmentSummaryAsync(Guid teacherId);

    #endregion
}
