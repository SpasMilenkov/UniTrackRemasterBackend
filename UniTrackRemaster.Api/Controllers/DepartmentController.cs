using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Department;
using UniTrackRemaster.Api.Dto.Teacher;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Services.Academics;

namespace UniTrackRemaster.Controllers;


[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(IDepartmentService departmentService, ILogger<DepartmentsController> logger)
    {
        _departmentService = departmentService;
        _logger = logger;
    }

    #region Base CRUD operations

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher,Student")]
    public async Task<ActionResult<DepartmentResponseDto>> GetById(Guid id)
    {
        return await _departmentService.GetByIdAsync(id);
    }

    /// <summary>
    /// Gets all departments with optional filtering and pagination (SuperAdmin only)
    /// </summary>
    /// <param name="query">Search term for department name, code, or description</param>
    /// <param name="facultyId">Filter by faculty ID</param>
    /// <param name="institutionId">Filter by institution ID</param>
    /// <param name="type">Filter by department type</param>
    /// <param name="status">Filter by department status</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <returns>Paginated list of all departments</returns>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(typeof(PagedResult<DepartmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<DepartmentResponseDto>>> GetAll(
        [FromQuery] string? query = null,
        [FromQuery] string? facultyId = null,
        [FromQuery] string? institutionId = null,
        [FromQuery] string? type = null,
        [FromQuery] string? status = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 50)
    {
        _logger.LogInformation("Attempting to retrieve all departments with filters - Query: {Query}, Page: {Page}, PageSize: {PageSize}", 
            query, page, pageSize);
        
        try
        {
            var departments = await _departmentService.GetAllAsync(
                query, facultyId, institutionId, type, status, page, pageSize);
            
            _logger.LogInformation("Successfully retrieved {Count} departments (Page {Page} of {TotalPages})", 
                departments.Items.Count, departments.CurrentPage, departments.TotalPages);
            return Ok(departments);
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving all departments");
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
    /// Gets all departments for a specific institution with optional filtering and pagination
    /// </summary>
    /// <param name="institutionId">The institution ID to filter departments by</param>
    /// <param name="query">Search term for department name, code, or description</param>
    /// <param name="facultyId">Filter by faculty ID within the institution</param>
    /// <param name="type">Filter by department type</param>
    /// <param name="status">Filter by department status</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <returns>Paginated list of departments for the institution</returns>
    /// <response code="200">Departments retrieved successfully</response>
    /// <response code="400">Invalid query parameters</response>
    /// <response code="404">Institution not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("institution/{institutionId}")]
    [ProducesResponseType(typeof(PagedResult<DepartmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<ActionResult<PagedResult<DepartmentResponseDto>>> GetByInstitution(
        [FromRoute] Guid institutionId,
        [FromQuery] string? query = null,
        [FromQuery] string? facultyId = null,
        [FromQuery] string? type = null,
        [FromQuery] string? status = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 50)
    {
        _logger.LogInformation("Attempting to retrieve departments for institution {InstitutionId} with filters - Query: {Query}, Page: {Page}, PageSize: {PageSize}", 
            institutionId, query, page, pageSize);
        
        try
        {
            var departments = await _departmentService.GetDepartmentsByInstitutionAsync(
                institutionId, query, facultyId, type, status, page, pageSize);
            
            if (departments.TotalCount == 0)
            {
                _logger.LogInformation("No departments found for institution {InstitutionId}", institutionId);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved {Count} departments for institution {InstitutionId} (Page {Page} of {TotalPages})", 
                    departments.Items.Count, institutionId, departments.CurrentPage, departments.TotalPages);
            }
            
            return Ok(departments);
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving departments for institution {InstitutionId}", institutionId);
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
    public async Task<ActionResult<DepartmentResponseDto>> Create(CreateDepartmentDto dto)
    {
        var department = await _departmentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<DepartmentResponseDto>> Update(Guid id, UpdateDepartmentDto dto)
    {
        return await _departmentService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _departmentService.DeleteAsync(id);
        return NoContent();
    }

    #endregion

    #region Non-paginated endpoints (for dropdowns, calculations, etc.)

    /// <summary>
    /// Gets all departments without pagination (for dropdowns, calculations)
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    [ProducesResponseType(typeof(IEnumerable<DepartmentResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DepartmentResponseDto>>> GetAllNonPaginated()
    {
        try
        {
            var departments = await _departmentService.GetAllAsync();
            return Ok(departments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all departments (non-paginated)");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets all departments by faculty without pagination (for dropdowns, calculations)
    /// </summary>
    [HttpGet("faculty/{facultyId}/all")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<DepartmentResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DepartmentResponseDto>>> GetByFacultyNonPaginated(Guid facultyId)
    {
        try
        {
            var departments = await _departmentService.GetByFacultyAsync(facultyId);
            return Ok(departments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving departments for faculty {FacultyId} (non-paginated)", facultyId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets all departments by institution without pagination (for dropdowns, calculations)
    /// </summary>
    [HttpGet("institution/{institutionId}/all")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<DepartmentResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DepartmentResponseDto>>> GetByInstitutionNonPaginated(Guid institutionId)
    {
        try
        {
            var departments = await _departmentService.GetByInstitutionIdAsync(institutionId);
            return Ok(departments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving departments for institution {InstitutionId} (non-paginated)", institutionId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    #endregion

    #region Department-specific endpoints

    /// <summary>
    /// Gets all teachers within a specific department
    /// </summary>
    [HttpGet("{departmentId}/teachers")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    [ProducesResponseType(typeof(IEnumerable<TeacherResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TeacherResponseDto>>> GetDepartmentTeachers(Guid departmentId)
    {
        try
        {
            var teachers = await _departmentService.GetDepartmentTeachersAsync(departmentId);
            return Ok(teachers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teachers for department {DepartmentId}", departmentId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
    
    /// <summary>
    /// Assigns a teacher to a department
    /// </summary>
    [HttpPost("{departmentId}/teachers/{teacherId}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult> AssignTeacher(Guid departmentId, Guid teacherId)
    {
        await _departmentService.AssignTeacherAsync(departmentId, teacherId);
        return NoContent();
    }

    /// <summary>
    /// Removes a teacher from a department
    /// </summary>
    [HttpDelete("{departmentId}/teachers/{teacherId}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult> RemoveTeacher(Guid departmentId, Guid teacherId)
    {
        await _departmentService.RemoveTeacherAsync(departmentId, teacherId);
        return NoContent();
    }

    #endregion
}