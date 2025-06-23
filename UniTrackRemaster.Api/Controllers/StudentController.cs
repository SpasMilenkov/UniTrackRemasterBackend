using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UniTrackRemaster.Api.Dto.Student;
using UniTrackRemaster.Api.Dto.Student.Analytics;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Services.User.Parents.Exceptions;

namespace UniTrackRemaster.Controllers;

/// <summary>
/// Student management and analytics controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IMarkAnalyticsService _markAnalyticsService;
    private readonly IParentService _parentService;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(
        IStudentService studentService,
        IMarkAnalyticsService markAnalyticsService,
        IParentService parentService,
        ILogger<StudentsController> logger)
    {
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _markAnalyticsService = markAnalyticsService ?? throw new ArgumentNullException(nameof(markAnalyticsService));
        _parentService = parentService ?? throw new ArgumentNullException(nameof(parentService)); // Add this
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Student Management

    /// <summary>
    ///     Get student by ID
    /// </summary>
    /// <param name="id">The unique identifier of the student</param>
    /// <returns>Student details</returns>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin,Student,Parent")]
    [SwaggerOperation(
        Summary = "Get student by ID",
        Description = "Retrieves detailed information about a specific student by their unique identifier.",
        OperationId = "GetStudentById")]
    [SwaggerResponse(200, "Student found", typeof(StudentResponseDto))]
    [SwaggerResponse(404, "Student not found")]
    [SwaggerResponse(403, "Access denied - insufficient permissions")]
    [SwaggerResponse(400, "Invalid student ID format")]
    public async Task<ActionResult<StudentResponseDto>> GetById(
        [FromRoute] [Required] Guid id)
    {
        _logger.LogInformation("Getting student details for ID: {StudentId}", id);

        if (!await CanAccessStudentData(id))
        {
            _logger.LogWarning("Access denied for student {StudentId} by user {UserId}",
                id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Forbid("You don't have permission to access this student's data");
        }

        var student = await _studentService.GetByIdAsync(id);
        return Ok(student);
    }

    /// <summary>
    ///     Get all students with pagination and filtering
    /// </summary>
    /// <param name="query">Search query for filtering students</param>
    /// <param name="gradeId">Filter by grade ID</param>
    /// <param name="institutionId">Filter by institution ID</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page (max 100)</param>
    /// <param name="sortBy">Field to sort by</param>
    /// <param name="ascending">Sort direction</param>
    /// <returns>Paginated list of students</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [SwaggerOperation(
        Summary = "Get students with pagination",
        Description = "Retrieves a paginated list of students with optional filtering and sorting.",
        OperationId = "GetStudents")]
    [SwaggerResponse(200, "Students retrieved successfully", typeof(PaginatedStudentResponseDto))]
    [SwaggerResponse(400, "Invalid pagination or filter parameters")]
    [SwaggerResponse(403, "Access denied - admin role required")]
    public async Task<ActionResult<PaginatedStudentResponseDto>> GetStudents(
        [FromQuery] [SwaggerParameter("Search query for student name, email, or institution")]
        string? query = null,
        [FromQuery] [SwaggerParameter("Filter by grade ID")]
        Guid? gradeId = null,
        [FromQuery] [SwaggerParameter("Filter by institution ID")]
        Guid? institutionId = null,
        [FromQuery] [Range(1, int.MaxValue)] [SwaggerParameter("Page number (1-based)")]
        int pageNumber = 1,
        [FromQuery] [Range(1, 100)] [SwaggerParameter("Number of items per page (max 100)")]
        int pageSize = 20,
        [FromQuery] [SwaggerParameter("Field to sort by (FirstName, LastName, Email, Grade, Institution, CreatedAt)")]
        string sortBy = "FirstName",
        [FromQuery] [SwaggerParameter("Sort in ascending order")]
        bool ascending = true)
    {
        try
        {
            _logger.LogInformation("Getting students list with query: {Query}, page: {Page}, size: {Size}",
                query, pageNumber, pageSize);

            var request = new StudentSearchRequestDto
            {
                Query = query,
                GradeId = gradeId,
                InstitutionId = institutionId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                Ascending = ascending
            };

            var result = await _studentService.SearchWithPaginationAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving students list");
            return StatusCode(500, "An error occurred while retrieving students");
        }
    }

    /// <summary>
    ///     Get students by school
    /// </summary>
    /// <param name="schoolId">The school ID</param>
    /// <returns>List of students in the school</returns>
    [HttpGet("school/{schoolId:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [SwaggerOperation(
        Summary = "Get students by school",
        Description = "Retrieves all students enrolled in a specific school.",
        OperationId = "GetStudentsBySchool")]
    [SwaggerResponse(200, "Students retrieved successfully", typeof(IEnumerable<StudentResponseDto>))]
    [SwaggerResponse(404, "School not found")]
    [SwaggerResponse(403, "Access denied - admin role required")]
    public async Task<ActionResult<IEnumerable<StudentResponseDto>>> GetBySchool(
        [FromRoute] [Required] Guid schoolId)
    {
        _logger.LogInformation("Getting students for school: {SchoolId}", schoolId);
        var students = await _studentService.GetBySchoolAsync(schoolId);
        return Ok(students);
    }

    /// <summary>
    ///     Get students by university
    /// </summary>
    /// <param name="universityId">The university ID</param>
    /// <returns>List of students in the university</returns>
    [HttpGet("university/{universityId:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [SwaggerOperation(
        Summary = "Get students by university",
        Description = "Retrieves all students enrolled in a specific university.",
        OperationId = "GetStudentsByUniversity")]
    [SwaggerResponse(200, "Students retrieved successfully", typeof(IEnumerable<StudentResponseDto>))]
    [SwaggerResponse(404, "University not found")]
    [SwaggerResponse(403, "Access denied - admin role required")]
    public async Task<ActionResult<IEnumerable<StudentResponseDto>>> GetByUniversity(
        [FromRoute] [Required] Guid universityId)
    {
        _logger.LogInformation("Getting students for university: {UniversityId}", universityId);
        var students = await _studentService.GetByUniversityAsync(universityId);
        return Ok(students);
    }

    /// <summary>
    ///     Get student by user ID
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Student associated with the user</returns>
    [HttpGet("user/{userId:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin,Student")]
    [SwaggerOperation(
        Summary = "Get student by user ID",
        Description = "Retrieves student information associated with a specific user account.",
        OperationId = "GetStudentByUserId")]
    [SwaggerResponse(200, "Student found", typeof(StudentResponseDto))]
    [SwaggerResponse(404, "Student not found for this user")]
    [SwaggerResponse(403, "Access denied - insufficient permissions")]
    public async Task<ActionResult<StudentResponseDto>> GetByUserId(
        [FromRoute] [Required] Guid userId)
    {
        _logger.LogInformation("Getting student for user: {UserId}", userId);
        var student = await _studentService.GetByUserIdAsync(userId);
        return Ok(student);
    }

    /// <summary>
    ///     Get students by grade
    /// </summary>
    /// <param name="gradeId">The grade ID</param>
    /// <returns>List of students in the grade</returns>
    [HttpGet("grade/{gradeId:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [SwaggerOperation(
        Summary = "Get students by grade",
        Description = "Retrieves all students enrolled in a specific grade/class.",
        OperationId = "GetStudentsByGrade")]
    [SwaggerResponse(200, "Students retrieved successfully", typeof(IEnumerable<StudentResponseDto>))]
    [SwaggerResponse(404, "Grade not found")]
    [SwaggerResponse(403, "Access denied - admin role required")]
    public async Task<ActionResult<IEnumerable<StudentResponseDto>>> GetByGrade(
        [FromRoute] [Required] Guid gradeId)
    {
        _logger.LogInformation("Getting students for grade: {GradeId}", gradeId);
        var students = await _studentService.GetByGradeAsync(gradeId);
        return Ok(students);
    }

    /// <summary>
    ///     Create a new student
    /// </summary>
    /// <param name="dto">Student creation data</param>
    /// <returns>Created student details</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [SwaggerOperation(
        Summary = "Create a new student",
        Description = "Creates a new student record with the provided information.",
        OperationId = "CreateStudent")]
    [SwaggerResponse(201, "Student created successfully", typeof(StudentResponseDto))]
    [SwaggerResponse(400, "Invalid student data")]
    [SwaggerResponse(409, "Student already exists")]
    [SwaggerResponse(403, "Access denied - admin role required")]
    public async Task<ActionResult<StudentResponseDto>> Create(
        [FromBody] [Required] CreateStudentDto dto)
    {
        _logger.LogInformation("Creating new student with email: {Email}", dto.Email);

        var student = await _studentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    /// <summary>
    ///     Update an existing student
    /// </summary>
    /// <param name="id">Student ID to update</param>
    /// <param name="dto">Updated student data</param>
    /// <returns>Updated student details</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [SwaggerOperation(
        Summary = "Update a student",
        Description = "Updates an existing student record with new information.",
        OperationId = "UpdateStudent")]
    [SwaggerResponse(200, "Student updated successfully", typeof(StudentResponseDto))]
    [SwaggerResponse(404, "Student not found")]
    [SwaggerResponse(400, "Invalid student data")]
    [SwaggerResponse(403, "Access denied - admin role required")]
    public async Task<ActionResult<StudentResponseDto>> Update(
        [FromRoute] [Required] Guid id,
        [FromBody] [Required] UpdateStudentDto dto)
    {
        _logger.LogInformation("Updating student: {StudentId}", id);

        var student = await _studentService.UpdateAsync(id, dto);
        return Ok(student);
    }

    /// <summary>
    ///     Delete a student
    /// </summary>
    /// <param name="id">Student ID to delete</param>
    /// <returns>No content on successful deletion</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [SwaggerOperation(
        Summary = "Delete a student",
        Description = "Permanently deletes a student record and all associated data.",
        OperationId = "DeleteStudent")]
    [SwaggerResponse(204, "Student deleted successfully")]
    [SwaggerResponse(404, "Student not found")]
    [SwaggerResponse(403, "Access denied - admin role required")]
    public async Task<ActionResult> Delete([FromRoute] [Required] Guid id)
    {
        _logger.LogInformation("Deleting student: {StudentId}", id);

        await _studentService.DeleteAsync(id);
        return NoContent();
    }

    #endregion

    #region Academic Analytics

    /// <summary>
    ///     Get student's grade dashboard with semester-aware analytics
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <param name="term">Specific semester term (Fall, Spring, Summer, etc.)</param>
    /// <param name="academicYear">Academic year (e.g., "2023-2024")</param>
    /// <returns>Comprehensive grade dashboard with semester breakdown</returns>
    [HttpGet("{id:guid}/grades/dashboard")]
    [Authorize(Roles = "Student,Admin,SuperAdmin,Parent")]
    [SwaggerOperation(
        Summary = "Get student grade dashboard",
        Description =
            "Retrieves a comprehensive grade dashboard with GPA, trends, rankings, and semester-aware analytics.",
        OperationId = "GetStudentGradeDashboard")]
    [SwaggerResponse(200, "Dashboard retrieved successfully", typeof(StudentGradeDashboardDto))]
    [SwaggerResponse(404, "Student not found")]
    [SwaggerResponse(403, "Access denied - insufficient permissions")]
    [SwaggerResponse(400, "Invalid parameters")]
    public async Task<ActionResult<StudentGradeDashboardDto>> GetGradesDashboard(
        [FromRoute] [Required] Guid id,
        [FromQuery] [SwaggerParameter("Semester term (Fall, Spring, Summer, etc.)")]
        SemesterType? term = null,
        [FromQuery] [SwaggerParameter("Academic year (e.g., '2023-2024')")]
        string? academicYear = null)
    {
        try
        {
            _logger.LogInformation(
                "Getting grade dashboard for student {StudentId}, term: {Term}, year: {Year}",
                id, term, academicYear);

            if (!await CanAccessStudentData(id))
                return Forbid("You don't have permission to access this student's academic data");

            var dashboard = await _markAnalyticsService.GetStudentGradeDashboard(
                id,
                term?.ToString(),
                academicYear);

            return Ok(dashboard);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found for student {StudentId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating grade dashboard for student {StudentId}", id);
            return StatusCode(500, "An error occurred while generating the grade dashboard");
        }
    }

    /// <summary>
    ///     Get student's complete academic transcript with semester breakdown
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <returns>Complete academic transcript with semester summaries</returns>
    [HttpGet("{id:guid}/grades/transcript")]
    [Authorize(Roles = "Student,Admin,SuperAdmin,Parent")]
    [SwaggerOperation(
        Summary = "Get student transcript",
        Description =
            "Retrieves the complete academic transcript with all courses, grades, credits, and semester summaries.",
        OperationId = "GetStudentTranscript")]
    [SwaggerResponse(200, "Transcript retrieved successfully", typeof(TranscriptDto))]
    [SwaggerResponse(404, "Student not found")]
    [SwaggerResponse(403, "Access denied - insufficient permissions")]
    public async Task<ActionResult<TranscriptDto>> GetTranscript(
        [FromRoute] [Required] Guid id)
    {
        try
        {
            _logger.LogInformation("Getting transcript for student {StudentId}", id);

            if (!await CanAccessStudentData(id))
                return Forbid("You don't have permission to access this student's transcript");

            var transcript = await _markAnalyticsService.GetStudentTranscript(id);
            return Ok(transcript);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Student not found: {StudentId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating transcript for student {StudentId}", id);
            return StatusCode(500, "An error occurred while generating the transcript");
        }
    }

    /// <summary>
    ///     Get student's grades for a specific semester
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <param name="term">Semester term (required)</param>
    /// <param name="academicYear">Academic year (required)</param>
    /// <returns>List of courses and grades for the specified semester</returns>
    [HttpGet("{id:guid}/grades/term")]
    [Authorize(Roles = "Student,Admin,SuperAdmin,Parent")]
    [SwaggerOperation(
        Summary = "Get grades for specific semester",
        Description = "Retrieves all courses and grades for a specific semester term.",
        OperationId = "GetStudentTermGrades")]
    [SwaggerResponse(200, "Term grades retrieved successfully", typeof(List<CourseGradeDto>))]
    [SwaggerResponse(400, "Missing required parameters (term and academic year)")]
    [SwaggerResponse(404, "Student or academic term not found")]
    [SwaggerResponse(403, "Access denied - insufficient permissions")]
    public async Task<ActionResult<List<CourseGradeDto>>> GetTermGrades(
        [FromRoute] [Required] Guid id,
        [FromQuery] [Required] [SwaggerParameter("Semester term (Fall, Spring, Summer, etc.)")]
        SemesterType term,
        [FromQuery] [Required] [SwaggerParameter("Academic year (e.g., '2023-2024')")]
        string academicYear)
    {
        try
        {
            _logger.LogInformation("Getting term grades for student {StudentId}, term: {Term}, year: {Year}",
                id, term, academicYear);

            if (!await CanAccessStudentData(id))
                return Forbid("You don't have permission to access this student's grades");

            if (string.IsNullOrWhiteSpace(academicYear)) return BadRequest("Academic year is required");

            var grades = await _markAnalyticsService.GetStudentTermGrades(
                id,
                term.ToString(),
                academicYear);

            return Ok(grades);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found for student {StudentId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving term grades for student {StudentId}", id);
            return StatusCode(500, "An error occurred while retrieving term grades");
        }
    }

    /// <summary>
    ///     Get attendance and performance correlation insights
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <param name="term">Specific semester term</param>
    /// <param name="academicYear">Academic year</param>
    /// <returns>Detailed attendance-performance analysis with improvement recommendations</returns>
    [HttpGet("{id:guid}/attendance/insight")]
    [Authorize(Roles = "Student,Admin,SuperAdmin,Parent")]
    [SwaggerOperation(
        Summary = "Get attendance-performance insights",
        Description =
            "Analyzes the correlation between attendance and academic performance with personalized recommendations.",
        OperationId = "GetAttendancePerformanceInsight")]
    [SwaggerResponse(200, "Insights retrieved successfully", typeof(AttendancePerformanceInsightDto))]
    [SwaggerResponse(404, "Student not found")]
    [SwaggerResponse(403, "Access denied - insufficient permissions")]
    public async Task<ActionResult<AttendancePerformanceInsightDto>> GetAttendanceInsight(
        [FromRoute] [Required] Guid id,
        [FromQuery] [SwaggerParameter("Semester term (Fall, Spring, Summer, etc.)")]
        SemesterType? term = null,
        [FromQuery] [SwaggerParameter("Academic year (e.g., '2023-2024')")]
        string? academicYear = null)
    {
        try
        {
            _logger.LogInformation(
                "Getting attendance insights for student {StudentId}, term: {Term}, year: {Year}",
                id, term, academicYear);

            if (!await CanAccessStudentData(id))
                return Forbid("You don't have permission to access this student's attendance data");

            var insight = await _markAnalyticsService.GetAttendancePerformanceInsight(
                id,
                term?.ToString(),
                academicYear);

            return Ok(insight);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found for student {StudentId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating attendance insights for student {StudentId}", id);
            return StatusCode(500, "An error occurred while calculating attendance insights");
        }
    }

    /// <summary>
    ///     Export student grades in specified format
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <param name="format">Export format (pdf or excel)</param>
    /// <param name="term">Specific semester term</param>
    /// <param name="academicYear">Academic year</param>
    /// <returns>Exported file with student grades</returns>
    [HttpGet("{id:guid}/grades/export")]
    [Authorize(Roles = "Student,Admin,SuperAdmin,Parent")]
    [SwaggerOperation(
        Summary = "Export student grades",
        Description = "Exports student grades and transcript data in PDF or Excel format.",
        OperationId = "ExportStudentGrades")]
    [SwaggerResponse(200, "File exported successfully", typeof(FileResult))]
    [SwaggerResponse(400, "Invalid format or missing parameters")]
    [SwaggerResponse(404, "Student not found")]
    [SwaggerResponse(403, "Access denied - insufficient permissions")]
    [SwaggerResponse(501, "Export functionality not implemented")]
    public async Task<IActionResult> ExportGrades(
        [FromRoute] [Required] Guid id,
        [FromQuery] [Required] [SwaggerParameter("Export format: 'pdf' or 'excel'")]
        string format,
        [FromQuery] [SwaggerParameter("Semester term (Fall, Spring, Summer, etc.)")]
        SemesterType? term = null,
        [FromQuery] [SwaggerParameter("Academic year (e.g., '2023-2024')")]
        string? academicYear = null)
    {
        try
        {
            _logger.LogInformation("Exporting grades for student {StudentId} in format {Format}", id, format);

            if (!await CanAccessStudentData(id))
                return Forbid("You don't have permission to export this student's grades");

            if (string.IsNullOrWhiteSpace(format) ||
                !new[] { "pdf", "excel" }.Contains(format.ToLower()))
                return BadRequest("Format must be 'pdf' or 'excel'");

            var fileBytes = await _markAnalyticsService.ExportGrades(
                id,
                format,
                term?.ToString(),
                academicYear);

            var contentType = format.ToLower() == "pdf"
                ? "application/pdf"
                : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            var fileName = $"student_{id}_grades_{DateTime.UtcNow:yyyyMMdd}.{format.ToLower()}";

            return File(fileBytes, contentType, fileName);
        }
        catch (NotImplementedException)
        {
            return StatusCode(501, "Export functionality is not yet implemented");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found for student {StudentId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting grades for student {StudentId}", id);
            return StatusCode(500, "An error occurred while exporting grades");
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    ///     Checks if the current user has access to student data
    /// </summary>
    /// <param name="studentId">Student ID to check access for</param>
    /// <returns>True if user has access, false otherwise</returns>
    private async Task<bool> CanAccessStudentData(Guid studentId)
    {
        try
        {
            // Get the current user ID from claims
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var userGuid))
            {
                _logger.LogWarning("No valid user ID found in claims");
                return false;
            }

            // Allow if user is admin or super admin
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
            {
                _logger.LogDebug("Access granted to admin user {UserId} for student {StudentId}",
                    currentUserId, studentId);
                return true;
            }

            // Get the student data
            var student = await _studentService.GetByIdAsync(studentId);
            if (student == null)
            {
                _logger.LogWarning("Student {StudentId} not found", studentId);
                return false;
            }

            // Allow if user is the student themselves
            var isOwnData = student.UserId == userGuid;
            if (isOwnData)
            {
                _logger.LogDebug("Access granted to student {StudentId} for their own data", studentId);
                return true;
            }

            // Allow if user is a parent of the student
            try
            {
                var parent = await _parentService.GetByUserIdAsync(userGuid);
                var children = await _parentService.GetChildrenAsync(parent.Id);

                var isParent = children.Any(child => child.Id == studentId);
                if (isParent)
                {
                    _logger.LogDebug("Access granted to parent {UserId} for their child {StudentId}",
                        currentUserId, studentId);
                    return true;
                }
            }
            catch (ParentNotFoundException)
            {
                // User is not a parent, which is fine - continue to deny access
                _logger.LogDebug("User {UserId} is not a parent", currentUserId);
            }

            _logger.LogWarning("Access denied for user {UserId} to student {StudentId} data",
                currentUserId, studentId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking access permissions for student {StudentId}", studentId);
            return false;
        }
    }

    #endregion
}