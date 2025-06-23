using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Semester;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Services.Academics;

namespace UniTrackRemaster.Controllers;

[ApiController]
[Route("api/academic-years/{academicYearId}/semesters")]
[Authorize]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _semesterService;
    private readonly ILogger<SemestersController> _logger;

    public SemestersController(ISemesterService semesterService, ILogger<SemestersController> logger)
    {
        _semesterService = semesterService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a specific semester by ID for an academic year
    /// </summary>
    /// <param name="academicYearId">The academic year ID</param>
    /// <param name="id">The semester ID</param>
    /// <returns>Semester details</returns>
    /// <response code="200">Semester retrieved successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="404">Semester not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin,Student,Teacher,Parent")]
    [ProducesResponseType(typeof(SemesterResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SemesterResponseDto>> GetById(Guid academicYearId, Guid id)
    {
        _logger.LogInformation("Attempting to retrieve semester {SemesterId} for academic year {AcademicYearId}", 
            id, academicYearId);

        try
        {
            var semester = await _semesterService.GetByIdAsync(id);
            
            // Verify this semester belongs to the specified academic year
            if (semester.AcademicYearId != academicYearId)
            {
                _logger.LogWarning("Semester {SemesterId} does not belong to academic year {AcademicYearId}", 
                    id, academicYearId);
                return BadRequest(new ProblemDetails
                {
                    Title = "Academic Year Mismatch",
                    Detail = $"Semester with ID {id} does not belong to academic year with ID {academicYearId}",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                });
            }

            _logger.LogInformation("Successfully retrieved semester {SemesterId} for academic year {AcademicYearId}", 
                id, academicYearId);
            return Ok(semester);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Semester {SemesterId} not found. Error: {Error}", id, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Semester Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving semester {SemesterId}", id);
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
    /// Gets all semesters for a specific academic year with optional filtering and pagination
    /// </summary>
    /// <param name="academicYearId">The academic year ID to filter semesters by</param>
    /// <param name="query">Search term for name or description</param>
    /// <param name="status">Filter by semester status</param>
    /// <param name="type">Filter by semester type</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <returns>Paginated list of semesters for the academic year</returns>
    /// <response code="200">Semesters retrieved successfully</response>
    /// <response code="400">Invalid query parameters</response>
    /// <response code="404">Academic year not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin,Student,Teacher,Parent")]
    [ProducesResponseType(typeof(PagedResult<SemesterResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<SemesterResponseDto>>> GetByAcademicYear(
        [FromRoute] Guid academicYearId,
        [FromQuery] string? query = null,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null,
        [FromQuery] bool? isActive = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 50)
    {
        _logger.LogInformation("Attempting to retrieve semesters for academic year {AcademicYearId} with filters - Query: {Query}, Page: {Page}, PageSize: {PageSize}", 
            academicYearId, query, page, pageSize);
        
        try
        {
            var semesters = await _semesterService.GetByAcademicYearAsync(
                academicYearId, query, status, type, isActive, page, pageSize);
            
            if (semesters.TotalCount == 0)
            {
                _logger.LogInformation("No semesters found for academic year {AcademicYearId}", academicYearId);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved {Count} semesters for academic year {AcademicYearId} (Page {Page} of {TotalPages})", 
                    semesters.Items.Count, academicYearId, semesters.CurrentPage, semesters.TotalPages);
            }
            
            return Ok(semesters);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid query parameters provided for academic year {AcademicYearId}. Error: {Error}", 
                academicYearId, ex.Message);
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Query Parameters",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Academic year {AcademicYearId} not found. Error: {Error}", academicYearId, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Academic Year Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving semesters for academic year {AcademicYearId}", academicYearId);
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
    /// Creates a new semester for a specific academic year
    /// </summary>
    /// <param name="academicYearId">The academic year ID</param>
    /// <param name="dto">Semester creation data</param>
    /// <returns>Created semester</returns>
    /// <response code="201">Semester created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="409">Semester already exists</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(SemesterResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SemesterResponseDto>> Create(Guid academicYearId, [FromBody] CreateSemesterDto dto)
    {
        _logger.LogInformation("Attempting to create semester for academic year {AcademicYearId} with name: {Name}", 
            academicYearId, dto.Name);

        try
        {
            // Ensure the academicYearId in the URL matches the one in the DTO
            if (dto.AcademicYearId != academicYearId)
            {
                _logger.LogWarning("Academic year ID mismatch. URL: {UrlId}, DTO: {DtoId}", academicYearId, dto.AcademicYearId);
                return BadRequest(new ProblemDetails
                {
                    Title = "Academic Year ID Mismatch",
                    Detail = "Academic year ID in the URL must match the one in the request body",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                });
            }

            var semester = await _semesterService.CreateAsync(dto);
            
            _logger.LogInformation("Successfully created semester {SemesterId} for academic year {AcademicYearId}", 
                semester.Id, academicYearId);
            
            return CreatedAtAction(nameof(GetById), new { academicYearId, id = semester.Id }, semester);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed while creating semester. Error: {Error}", ex.Message);
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Semester already exists or conflict occurred. Error: {Error}", ex.Message);
            return Conflict(new ProblemDetails
            {
                Title = "Semester Conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Academic year {AcademicYearId} not found. Error: {Error}", academicYearId, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Academic Year Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating semester for academic year {AcademicYearId}", academicYearId);
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
    /// Updates a specific semester
    /// </summary>
    /// <param name="academicYearId">The academic year ID</param>
    /// <param name="id">The semester ID</param>
    /// <param name="dto">Semester update data</param>
    /// <returns>Updated semester</returns>
    /// <response code="200">Semester updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Semester not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(SemesterResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SemesterResponseDto>> Update(Guid academicYearId, Guid id, [FromBody] UpdateSemesterDto dto)
    {
        _logger.LogInformation("Attempting to update semester {SemesterId} for academic year {AcademicYearId}", 
            id, academicYearId);

        try
        {
            // Get the existing semester first to verify it belongs to the specified academic year
            var existingSemester = await _semesterService.GetByIdAsync(id);
            if (existingSemester.AcademicYearId != academicYearId)
            {
                _logger.LogWarning("Semester {SemesterId} does not belong to academic year {AcademicYearId}", 
                    id, academicYearId);
                return BadRequest(new ProblemDetails
                {
                    Title = "Academic Year Mismatch",
                    Detail = $"Semester with ID {id} does not belong to academic year with ID {academicYearId}",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                });
            }

            var semester = await _semesterService.UpdateAsync(id, dto);
            
            _logger.LogInformation("Successfully updated semester {SemesterId} for academic year {AcademicYearId}", 
                id, academicYearId);
            
            return Ok(semester);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Semester {SemesterId} not found. Error: {Error}", id, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Semester Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed while updating semester {SemesterId}. Error: {Error}", 
                id, ex.Message);
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating semester {SemesterId}", id);
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
    /// Deletes a specific semester
    /// </summary>
    /// <param name="academicYearId">The academic year ID</param>
    /// <param name="id">The semester ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Semester deleted successfully</response>
    /// <response code="400">Invalid request or semester has dependencies</response>
    /// <response code="404">Semester not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(Guid academicYearId, Guid id)
    {
        _logger.LogInformation("Attempting to delete semester {SemesterId} for academic year {AcademicYearId}", 
            id, academicYearId);

        try
        {
            // Get the existing semester first to verify it belongs to the specified academic year
            var existingSemester = await _semesterService.GetByIdAsync(id);
            if (existingSemester.AcademicYearId != academicYearId)
            {
                _logger.LogWarning("Semester {SemesterId} does not belong to academic year {AcademicYearId}", 
                    id, academicYearId);
                return BadRequest(new ProblemDetails
                {
                    Title = "Academic Year Mismatch",
                    Detail = $"Semester with ID {id} does not belong to academic year with ID {academicYearId}",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                });
            }

            await _semesterService.DeleteAsync(id);
            
            _logger.LogInformation("Successfully deleted semester {SemesterId} for academic year {AcademicYearId}", 
                id, academicYearId);
            
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Semester {SemesterId} not found. Error: {Error}", id, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Semester Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Cannot delete semester {SemesterId} due to dependencies. Error: {Error}", 
                id, ex.Message);
            return BadRequest(new ProblemDetails
            {
                Title = "Cannot Delete Semester",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting semester {SemesterId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while processing your request.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = HttpContext.Request.Path
            });
        }
    }

    #region Additional endpoints for semester utilities

    /// <summary>
    /// Gets all semesters for an academic year without pagination (for dropdowns, calculations)
    /// </summary>
    /// <param name="academicYearId">The academic year ID</param>
    /// <returns>List of semesters</returns>
    [HttpGet("all")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    [ProducesResponseType(typeof(IEnumerable<SemesterResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SemesterResponseDto>>> GetAllNonPaginated(Guid academicYearId)
    {
        try
        {
            var semesters = await _semesterService.GetAllByAcademicYearAsync(academicYearId);
            return Ok(semesters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all semesters (non-paginated) for academic year {AcademicYearId}", academicYearId);
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
    /// Gets the current active semester for an academic year
    /// </summary>
    /// <param name="academicYearId">The academic year ID</param>
    /// <returns>Current semester</returns>
    [HttpGet("current")]
    [Authorize(Roles = "Admin,SuperAdmin,Student,Teacher,Parent")]
    [ProducesResponseType(typeof(SemesterResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SemesterResponseDto>> GetCurrent(Guid academicYearId)
    {
        try
        {
            var currentSemester = await _semesterService.GetCurrentAsync(academicYearId);
            return Ok(currentSemester);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("No current semester found for academic year {AcademicYearId}. Error: {Error}", 
                academicYearId, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Current Semester Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current semester for academic year {AcademicYearId}", academicYearId);
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
    /// Gets all active semesters for an academic year
    /// </summary>
    /// <param name="academicYearId">The academic year ID</param>
    /// <returns>List of active semesters</returns>
    [HttpGet("active")]
    [Authorize(Roles = "Admin,SuperAdmin,Student,Teacher,Parent")]
    [ProducesResponseType(typeof(IEnumerable<SemesterResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SemesterResponseDto>>> GetActive(Guid academicYearId)
    {
        try
        {
            var activeSemesters = await _semesterService.GetActiveAsync(academicYearId);
            return Ok(activeSemesters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active semesters for academic year {AcademicYearId}", academicYearId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while processing your request.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = HttpContext.Request.Path
            });
        }
    }

    #endregion
}