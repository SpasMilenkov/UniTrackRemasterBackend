using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Grade;
using UniTrackRemaster.Api.Dto.Student;
using UniTrackRemaster.Api.Dto.Teacher;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Services.Academics;

namespace UniTrackRemaster.Controllers
{
[ApiController]
[Route("api/[controller]")]
public class GradesController : ControllerBase
{
    private readonly IGradeService _gradeService;
    private readonly ILogger<GradesController> _logger;

    public GradesController(IGradeService gradeService, ILogger<GradesController> logger)
    {
        _gradeService = gradeService;
        _logger = logger;
    }

    #region Base CRUD operations

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher,Student")]
    public async Task<ActionResult<GradeResponseDto>> GetById(Guid id)
    {
        return await _gradeService.GetByIdAsync(id);
    }

    /// <summary>
    /// Gets all grades with optional filtering and pagination (SuperAdmin only)
    /// </summary>
    /// <param name="query">Search term for grade name</param>
    /// <param name="institutionId">Filter by institution ID</param>
    /// <param name="academicYearId">Filter by academic year ID</param>
    /// <param name="hasHomeRoomTeacher">Filter by homeroom teacher assignment</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <returns>Paginated list of all grades</returns>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(typeof(PagedResult<GradeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<GradeResponseDto>>> GetAll(
        [FromQuery] string? query = null,
        [FromQuery] string? institutionId = null,
        [FromQuery] string? academicYearId = null,
        [FromQuery] bool? hasHomeRoomTeacher = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 50)
    {
        _logger.LogInformation("Attempting to retrieve all grades with filters - Query: {Query}, Page: {Page}, PageSize: {PageSize}", 
            query, page, pageSize);
        
        try
        {
            var grades = await _gradeService.GetAllAsync(
                query, institutionId, academicYearId, hasHomeRoomTeacher, page, pageSize);
            
            _logger.LogInformation("Successfully retrieved {Count} grades (Page {Page} of {TotalPages})", 
                grades.Items.Count, grades.CurrentPage, grades.TotalPages);
            return Ok(grades);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid query parameters provided. Error: {Error}", ex.Message);
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Query Parameters",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving all grades");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while processing your request.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = HttpContext.Request.Path
            });
        }
    }

    /// <summary>
    /// Gets all grades for a specific institution with optional filtering and pagination
    /// </summary>
    /// <param name="institutionId">The institution ID to filter grades by</param>
    /// <param name="query">Search term for grade name</param>
    /// <param name="academicYearId">Filter by academic year ID</param>
    /// <param name="hasHomeRoomTeacher">Filter by homeroom teacher assignment</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <returns>Paginated list of grades for the institution</returns>
    /// <response code="200">Grades retrieved successfully</response>
    /// <response code="400">Invalid query parameters</response>
    /// <response code="404">Institution not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("institution/{institutionId}")]
    [ProducesResponseType(typeof(PagedResult<GradeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<ActionResult<PagedResult<GradeResponseDto>>> GetByInstitution(
        [FromRoute] Guid institutionId,
        [FromQuery] string? query = null,
        [FromQuery] string? academicYearId = null,
        [FromQuery] bool? hasHomeRoomTeacher = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 50)
    {
        _logger.LogInformation("Attempting to retrieve grades for institution {InstitutionId} with filters - Query: {Query}, Page: {Page}, PageSize: {PageSize}", 
            institutionId, query, page, pageSize);
        
        try
        {
            var grades = await _gradeService.GetGradesByInstitutionAsync(
                institutionId, query, academicYearId, hasHomeRoomTeacher, page, pageSize);
            
            if (grades.TotalCount == 0)
            {
                _logger.LogInformation("No grades found for institution {InstitutionId}", institutionId);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved {Count} grades for institution {InstitutionId} (Page {Page} of {TotalPages})", 
                    grades.Items.Count, institutionId, grades.CurrentPage, grades.TotalPages);
            }
            
            return Ok(grades);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid query parameters provided for institution {InstitutionId}. Error: {Error}", 
                institutionId, ex.Message);
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Query Parameters",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving grades for institution {InstitutionId}", institutionId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while processing your request.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = HttpContext.Request.Path
            });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<GradeResponseDto>> Create(CreateGradeDto dto)
    {
        var grade = await _gradeService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = grade.Id }, grade);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<GradeResponseDto>> Update(Guid id, UpdateGradeDto dto)
    {
        return await _gradeService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _gradeService.DeleteAsync(id);
        return NoContent();
    }

    #endregion

    #region Non-paginated endpoints (for dropdowns, calculations, etc.)

    /// <summary>
    /// Gets all grades without pagination (for dropdowns, calculations)
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    [ProducesResponseType(typeof(IEnumerable<GradeResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GradeResponseDto>>> GetAllNonPaginated()
    {
        try
        {
            var grades = await _gradeService.GetAllAsync();
            return Ok(grades);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all grades (non-paginated)");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets all grades by institution without pagination (for dropdowns, calculations)
    /// </summary>
    [HttpGet("institution/{institutionId}/all")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<GradeResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GradeResponseDto>>> GetByInstitutionNonPaginated(Guid institutionId)
    {
        try
        {
            var grades = await _gradeService.GetByInstitutionIdAsync(institutionId);
            return Ok(grades);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving grades for institution {InstitutionId} (non-paginated)", institutionId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets all grades by academic year without pagination (for dropdowns, calculations)
    /// </summary>
    [HttpGet("academic-year/{academicYearId}/all")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<GradeResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GradeResponseDto>>> GetByAcademicYearNonPaginated(Guid academicYearId)
    {
        try
        {
            var grades = await _gradeService.GetByAcademicYearIdAsync(academicYearId);
            return Ok(grades);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving grades for academic year {AcademicYearId} (non-paginated)", academicYearId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    #endregion

    #region Grade-specific endpoints

    /// <summary>
    /// Gets all teachers assigned to a specific grade
    /// </summary>
    [HttpGet("{gradeId}/teachers")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    [ProducesResponseType(typeof(IEnumerable<TeacherResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TeacherResponseDto>>> GetGradeTeachers(Guid gradeId)
    {
        try
        {
            var teachers = await _gradeService.GetAssignedTeachersAsync(gradeId);
            return Ok(teachers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teachers for grade {GradeId}", gradeId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets all students in a specific grade
    /// </summary>
    [HttpGet("{gradeId}/students")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    [ProducesResponseType(typeof(IEnumerable<StudentResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StudentResponseDto>>> GetGradeStudents(Guid gradeId)
    {
        try
        {
            var students = await _gradeService.GetGradeStudentsAsync(gradeId);
            return Ok(students);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving students for grade {GradeId}", gradeId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Assigns a teacher to a grade
    /// </summary>
    [HttpPost("{gradeId}/teachers/{teacherId}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult> AssignTeacher(Guid gradeId, Guid teacherId)
    {
        await _gradeService.AssignTeacherAsync(gradeId, teacherId);
        return NoContent();
    }

    /// <summary>
    /// Removes a teacher from a grade
    /// </summary>
    [HttpDelete("{gradeId}/teachers/{teacherId}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult> RemoveTeacher(Guid gradeId, Guid teacherId)
    {
        await _gradeService.RemoveTeacherAsync(gradeId, teacherId);
        return NoContent();
    }

    #endregion
}
}
