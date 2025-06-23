using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Subject;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Services.Academics;

namespace UniTrackRemaster.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;
    private readonly ILogger<SubjectsController> _logger;

    public SubjectsController(ISubjectService subjectService, ILogger<SubjectsController> logger)
    {
        _subjectService = subjectService;
        _logger = logger;
    }

    #region Base CRUD operations

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher,Student")]
    public async Task<ActionResult<SubjectResponseDto>> GetById(Guid id)
    {
        return await _subjectService.GetByIdAsync(id);
    }

    /// <summary>
    /// Gets all subjects with optional filtering and pagination (SuperAdmin only)
    /// </summary>
    /// <param name="query">Search term for name, code, or description</param>
    /// <param name="departmentId">Filter by department ID</param>
    /// <param name="academicLevel">Filter by academic level</param>
    /// <param name="electiveType">Filter by elective type</param>
    /// <param name="hasLab">Filter by lab requirement</param>
    /// <param name="isElective">Filter by elective status</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <returns>Paginated list of all subjects</returns>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(typeof(PagedResult<SubjectResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<SubjectResponseDto>>> GetAll(
        [FromQuery] string? query = null,
        [FromQuery] string? departmentId = null,
        [FromQuery] string? academicLevel = null,
        [FromQuery] string? electiveType = null,
        [FromQuery] bool? hasLab = null,
        [FromQuery] bool? isElective = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 50)
    {
        _logger.LogInformation("Attempting to retrieve all subjects with filters - Query: {Query}, Page: {Page}, PageSize: {PageSize}", 
            query, page, pageSize);
        
        try
        {
            var subjects = await _subjectService.GetAllAsync(
                query, departmentId, academicLevel, electiveType, hasLab, isElective, page, pageSize);
            
            _logger.LogInformation("Successfully retrieved {Count} subjects (Page {Page} of {TotalPages})", 
                subjects.Items.Count, subjects.CurrentPage, subjects.TotalPages);
            return Ok(subjects);
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving all subjects");
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
    /// Gets all subjects for a specific institution with optional filtering and pagination
    /// </summary>
    /// <param name="institutionId">The institution ID to filter subjects by</param>
    /// <param name="query">Search term for name, code, or description</param>
    /// <param name="departmentId">Filter by department ID</param>
    /// <param name="academicLevel">Filter by academic level</param>
    /// <param name="electiveType">Filter by elective type</param>
    /// <param name="hasLab">Filter by lab requirement</param>
    /// <param name="isElective">Filter by elective status</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <returns>Paginated list of subjects for the institution</returns>
    /// <response code="200">Subjects retrieved successfully</response>
    /// <response code="400">Invalid query parameters</response>
    /// <response code="404">Institution not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("institution/{institutionId}")]
    [ProducesResponseType(typeof(PagedResult<SubjectResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<ActionResult<PagedResult<SubjectResponseDto>>> GetByInstitution(
        [FromRoute] Guid institutionId,
        [FromQuery] string? query = null,
        [FromQuery] string? departmentId = null,
        [FromQuery] string? academicLevel = null,
        [FromQuery] string? electiveType = null,
        [FromQuery] bool? hasLab = null,
        [FromQuery] bool? isElective = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 50)
    {
        _logger.LogInformation("Attempting to retrieve subjects for institution {InstitutionId} with filters - Query: {Query}, Page: {Page}, PageSize: {PageSize}", 
            institutionId, query, page, pageSize);
        
        try
        {
            var subjects = await _subjectService.GetSubjectsByInstitutionAsync(
                institutionId, query, departmentId, academicLevel, electiveType, hasLab, isElective, page, pageSize);
            
            if (subjects.TotalCount == 0)
            {
                _logger.LogInformation("No subjects found for institution {InstitutionId}", institutionId);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved {Count} subjects for institution {InstitutionId} (Page {Page} of {TotalPages})", 
                    subjects.Items.Count, institutionId, subjects.CurrentPage, subjects.TotalPages);
            }
            
            return Ok(subjects);
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving subjects for institution {InstitutionId}", institutionId);
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
    public async Task<ActionResult<SubjectResponseDto>> Create(CreateSubjectDto dto)
    {
        var subject = await _subjectService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = subject.Id }, subject);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<SubjectResponseDto>> Update(Guid id, UpdateSubjectDto dto)
    {
        return await _subjectService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _subjectService.DeleteAsync(id);
        return NoContent();
    }

    #endregion

    #region Non-paginated endpoints (for dropdowns, calculations, etc.)

    /// <summary>
    /// Gets all subjects without pagination (for dropdowns, calculations)
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    [ProducesResponseType(typeof(IEnumerable<SubjectResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SubjectResponseDto>>> GetAllNonPaginated()
    {
        try
        {
            var subjects = await _subjectService.GetAllAsync();
            return Ok(subjects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all subjects (non-paginated)");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("department/{departmentId}")]
    [ProducesResponseType(typeof(IEnumerable<SubjectResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SubjectResponseDto>>> GetByDepartment(Guid departmentId)
    {
        try
        {
            var subjects = await _subjectService.GetByDepartmentAsync(departmentId);
            return Ok(subjects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subjects for department {DepartmentId}", departmentId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    #endregion

    #region Elective-specific endpoints

    [HttpGet("electives")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<SubjectResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SubjectResponseDto>>> GetElectives([FromQuery] bool activeOnly = true)
    {
        try
        {
            var electives = await _subjectService.GetElectivesAsync(activeOnly);
            return Ok(electives);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving elective subjects");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("electives/{subjectId}/enroll/{studentId}")]
    [Authorize(Roles = "Admin,SuperAdmin,Student")]
    public async Task<ActionResult> EnrollStudent(Guid subjectId, Guid studentId)
    {
        await _subjectService.EnrollStudentInElectiveAsync(subjectId, studentId);
        return NoContent();
    }

    [HttpPost("electives/{subjectId}/unenroll/{studentId}")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher,Student")]
    public async Task<ActionResult> UnenrollStudent(Guid subjectId, Guid studentId)
    {
        await _subjectService.UnenrollStudentFromElectiveAsync(subjectId, studentId);
        return NoContent();
    }

    [HttpGet("electives/{subjectId}/enrollments")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    public async Task<ActionResult<IEnumerable<StudentElectiveResponseDto>>> GetElectiveEnrollments(Guid subjectId)
    {
        var enrollments = await _subjectService.GetElectiveEnrollmentsAsync(subjectId);
        return Ok(enrollments);
    }

    [HttpGet("student/{studentId}/electives")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher,Student")]
    public async Task<ActionResult<IEnumerable<SubjectResponseDto>>> GetStudentElectives(Guid studentId)
    {
        var electives = await _subjectService.GetStudentElectivesAsync(studentId);
        return Ok(electives);
    }

    #endregion
}