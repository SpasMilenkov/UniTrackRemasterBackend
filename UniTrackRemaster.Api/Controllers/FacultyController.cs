using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Department;
using UniTrackRemaster.Api.Dto.Faculty;
using UniTrackRemaster.Api.Dto.Major;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Services.Academics;

namespace UniTrackRemaster.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacultiesController : ControllerBase
{
    private readonly IFacultyService _facultyService;
    private readonly ILogger<FacultiesController> _logger;

    public FacultiesController(IFacultyService facultyService, ILogger<FacultiesController> logger)
    {
        _facultyService = facultyService;
        _logger = logger;
    }

    #region Base CRUD operations

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher,Student")]
    public async Task<ActionResult<FacultyResponseDto>> GetById(Guid id)
    {
        return await _facultyService.GetByIdAsync(id);
    }

    /// <summary>
    /// Gets all faculties with optional filtering and pagination (SuperAdmin only)
    /// </summary>
    /// <param name="query">Search term for faculty name, code, or description</param>
    /// <param name="universityId">Filter by university ID</param>
    /// <param name="institutionId">Filter by institution ID</param>
    /// <param name="status">Filter by faculty status</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <returns>Paginated list of all faculties</returns>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(typeof(PagedResult<FacultyResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<FacultyResponseDto>>> GetAll(
        [FromQuery] string? query = null,
        [FromQuery] string? universityId = null,
        [FromQuery] string? institutionId = null,
        [FromQuery] string? status = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 50)
    {
        _logger.LogInformation("Attempting to retrieve all faculties with filters - Query: {Query}, Page: {Page}, PageSize: {PageSize}", 
            query, page, pageSize);
        
        try
        {
            var faculties = await _facultyService.GetAllAsync(
                query, universityId, institutionId, status, page, pageSize);
            
            _logger.LogInformation("Successfully retrieved {Count} faculties (Page {Page} of {TotalPages})", 
                faculties.Items.Count, faculties.CurrentPage, faculties.TotalPages);
            return Ok(faculties);
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving all faculties");
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
    /// Gets all faculties for a specific institution with optional filtering and pagination
    /// </summary>
    /// <param name="institutionId">The institution ID to filter faculties by</param>
    /// <param name="query">Search term for faculty name, code, or description</param>
    /// <param name="universityId">Filter by university ID within the institution</param>
    /// <param name="status">Filter by faculty status</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <returns>Paginated list of faculties for the institution</returns>
    /// <response code="200">Faculties retrieved successfully</response>
    /// <response code="400">Invalid query parameters</response>
    /// <response code="404">Institution not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("institution/{institutionId}")]
    [ProducesResponseType(typeof(PagedResult<FacultyResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<ActionResult<PagedResult<FacultyResponseDto>>> GetByInstitution(
        [FromRoute] Guid institutionId,
        [FromQuery] string? query = null,
        [FromQuery] string? universityId = null,
        [FromQuery] string? status = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 50)
    {
        _logger.LogInformation("Attempting to retrieve faculties for institution {InstitutionId} with filters - Query: {Query}, Page: {Page}, PageSize: {PageSize}", 
            institutionId, query, page, pageSize);
        
        try
        {
            var faculties = await _facultyService.GetFacultiesByInstitutionAsync(
                institutionId, query, universityId, status, page, pageSize);
            
            if (faculties.TotalCount == 0)
            {
                _logger.LogInformation("No faculties found for institution {InstitutionId}", institutionId);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved {Count} faculties for institution {InstitutionId} (Page {Page} of {TotalPages})", 
                    faculties.Items.Count, institutionId, faculties.CurrentPage, faculties.TotalPages);
            }
            
            return Ok(faculties);
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving faculties for institution {InstitutionId}", institutionId);
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
    public async Task<ActionResult<FacultyResponseDto>> Create(CreateFacultyDto dto)
    {
        var faculty = await _facultyService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = faculty.Id }, faculty);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<FacultyResponseDto>> Update(Guid id, UpdateFacultyDto dto)
    {
        return await _facultyService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _facultyService.DeleteAsync(id);
        return NoContent();
    }

    #endregion

    #region Non-paginated endpoints (for dropdowns, calculations, etc.)

    /// <summary>
    /// Gets all faculties without pagination (for dropdowns, calculations)
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    [ProducesResponseType(typeof(IEnumerable<FacultyResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FacultyResponseDto>>> GetAllNonPaginated()
    {
        try
        {
            var faculties = await _facultyService.GetAllAsync();
            return Ok(faculties);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all faculties (non-paginated)");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets all faculties by university without pagination (for dropdowns, calculations)
    /// </summary>
    [HttpGet("university/{universityId}/all")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<FacultyResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FacultyResponseDto>>> GetByUniversityNonPaginated(Guid universityId)
    {
        try
        {
            var faculties = await _facultyService.GetByUniversityAsync(universityId);
            return Ok(faculties);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving faculties for university {UniversityId} (non-paginated)", universityId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets all faculties by institution without pagination (for dropdowns, calculations)
    /// </summary>
    [HttpGet("institution/{institutionId}/all")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<FacultyResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FacultyResponseDto>>> GetByInstitutionNonPaginated(Guid institutionId)
    {
        try
        {
            var faculties = await _facultyService.GetByInstitutionIdAsync(institutionId);
            return Ok(faculties);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving faculties for institution {InstitutionId} (non-paginated)", institutionId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    #endregion

    #region Faculty-specific endpoints

    /// <summary>
    /// Gets all departments within a specific faculty
    /// </summary>
    [HttpGet("{facultyId}/departments")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    [ProducesResponseType(typeof(IEnumerable<DepartmentResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DepartmentResponseDto>>> GetFacultyDepartments(Guid facultyId)
    {
        try
        {
            var departments = await _facultyService.GetFacultyDepartmentsAsync(facultyId);
            return Ok(departments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving departments for faculty {FacultyId}", facultyId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets all majors within a specific faculty
    /// </summary>
    [HttpGet("{facultyId}/majors")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    [ProducesResponseType(typeof(IEnumerable<MajorResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MajorResponseDto>>> GetFacultyMajors(Guid facultyId)
    {
        try
        {
            var majors = await _facultyService.GetFacultyMajorsAsync(facultyId);
            return Ok(majors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving majors for faculty {FacultyId}", facultyId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    #endregion
}
