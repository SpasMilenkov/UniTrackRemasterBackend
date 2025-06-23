using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.AcademicYear;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Services.Academics;

namespace UniTrackRemaster.Controllers;

[ApiController]
[Route("api/institutions/{institutionId}/academic-years")]
[Authorize]
public class AcademicYearsController : ControllerBase
{
    private readonly IAcademicYearService _academicYearService;
    private readonly ILogger<AcademicYearsController> _logger;

    public AcademicYearsController(IAcademicYearService academicYearService, ILogger<AcademicYearsController> logger)
    {
        _academicYearService = academicYearService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a specific academic year by ID for an institution
    /// </summary>
    /// <param name="institutionId">The institution ID</param>
    /// <param name="id">The academic year ID</param>
    /// <returns>Academic year details</returns>
    /// <response code="200">Academic year retrieved successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="404">Academic year not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin,Student,Teacher,Parent")]
    [ProducesResponseType(typeof(AcademicYearResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AcademicYearResponseDto>> GetById(Guid institutionId, Guid id)
    {
        _logger.LogInformation("Attempting to retrieve academic year {AcademicYearId} for institution {InstitutionId}", 
            id, institutionId);

        try
        {
            var academicYear = await _academicYearService.GetByIdAsync(id);
            
            // Verify the academic year belongs to the specified institution
            if (academicYear.InstitutionId != institutionId)
            {
                _logger.LogWarning("Academic year {AcademicYearId} does not belong to institution {InstitutionId}", 
                    id, institutionId);
                return BadRequest(new ProblemDetails
                {
                    Title = "Institution Mismatch",
                    Detail = $"Academic year with ID {id} does not belong to institution with ID {institutionId}",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                });
            }

            _logger.LogInformation("Successfully retrieved academic year {AcademicYearId} for institution {InstitutionId}", 
                id, institutionId);
            return Ok(academicYear);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Academic year {AcademicYearId} not found. Error: {Error}", id, ex.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving academic year {AcademicYearId}", id);
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
    /// Gets all academic years for a specific institution with optional filtering and pagination
    /// </summary>
    /// <param name="institutionId">The institution ID to filter academic years by</param>
    /// <param name="query">Search term for name or description</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="isCurrent">Filter by current status (within date range)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <returns>Paginated list of academic years for the institution</returns>
    /// <response code="200">Academic years retrieved successfully</response>
    /// <response code="400">Invalid query parameters</response>
    /// <response code="404">Institution not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin,Student,Teacher,Parent")]
    [ProducesResponseType(typeof(PagedResult<AcademicYearResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<AcademicYearResponseDto>>> GetByInstitution(
        [FromRoute] Guid institutionId,
        [FromQuery] string? query = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? isCurrent = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 50)
    {
        _logger.LogInformation("Attempting to retrieve academic years for institution {InstitutionId} with filters - Query: {Query}, Page: {Page}, PageSize: {PageSize}", 
            institutionId, query, page, pageSize);
        
        try
        {
            var academicYears = await _academicYearService.GetByInstitutionAsync(
                institutionId, query, isActive, isCurrent, page, pageSize);
            
            if (academicYears.TotalCount == 0)
            {
                _logger.LogInformation("No academic years found for institution {InstitutionId}", institutionId);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved {Count} academic years for institution {InstitutionId} (Page {Page} of {TotalPages})", 
                    academicYears.Items.Count, institutionId, academicYears.CurrentPage, academicYears.TotalPages);
            }
            
            return Ok(academicYears);
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
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Institution {InstitutionId} not found. Error: {Error}", institutionId, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Institution Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving academic years for institution {InstitutionId}", institutionId);
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
    /// Creates a new academic year for a specific institution
    /// </summary>
    /// <param name="institutionId">The institution ID</param>
    /// <param name="dto">Academic year creation data</param>
    /// <returns>Created academic year</returns>
    /// <response code="201">Academic year created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="409">Academic year already exists</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(AcademicYearResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AcademicYearResponseDto>> Create(Guid institutionId, CreateAcademicYearDto dto)
    {
        _logger.LogInformation("Attempting to create academic year for institution {InstitutionId} with name: {Name}", 
            institutionId, dto.Name);

        try
        {
            // Ensure the institutionId in the URL matches the one in the DTO
            if (institutionId != dto.InstitutionId)
            {
                _logger.LogWarning("Institution ID mismatch. URL: {UrlId}, DTO: {DtoId}", institutionId, dto.InstitutionId);
                return BadRequest(new ProblemDetails
                {
                    Title = "Institution ID Mismatch",
                    Detail = "Institution ID in the URL must match the one in the request body",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                });
            }

            var academicYear = await _academicYearService.CreateAsync(dto);
            
            _logger.LogInformation("Successfully created academic year {AcademicYearId} for institution {InstitutionId}", 
                academicYear.Id, institutionId);
            
            return CreatedAtAction(nameof(GetById),
                new { institutionId, id = academicYear.Id },
                academicYear);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed while creating academic year. Error: {Error}", ex.Message);
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
            _logger.LogWarning("Academic year already exists or conflict occurred. Error: {Error}", ex.Message);
            return Conflict(new ProblemDetails
            {
                Title = "Academic Year Conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating academic year for institution {InstitutionId}", institutionId);
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
    /// Updates a specific academic year
    /// </summary>
    /// <param name="institutionId">The institution ID</param>
    /// <param name="id">The academic year ID</param>
    /// <param name="dto">Academic year update data</param>
    /// <returns>Updated academic year</returns>
    /// <response code="200">Academic year updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Academic year not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(AcademicYearResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AcademicYearResponseDto>> Update(
        Guid institutionId,
        Guid id,
        UpdateAcademicYearDto dto)
    {
        _logger.LogInformation("Attempting to update academic year {AcademicYearId} for institution {InstitutionId}", 
            id, institutionId);

        try
        {
            // Get the existing academic year first to verify it belongs to the specified institution
            var existingAcademicYear = await _academicYearService.GetByIdAsync(id);
            if (existingAcademicYear.InstitutionId != institutionId)
            {
                _logger.LogWarning("Academic year {AcademicYearId} does not belong to institution {InstitutionId}", 
                    id, institutionId);
                return BadRequest(new ProblemDetails
                {
                    Title = "Institution Mismatch",
                    Detail = $"Academic year with ID {id} does not belong to institution with ID {institutionId}",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                });
            }

            var academicYear = await _academicYearService.UpdateAsync(id, dto);
            
            _logger.LogInformation("Successfully updated academic year {AcademicYearId} for institution {InstitutionId}", 
                id, institutionId);
            
            return Ok(academicYear);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Academic year {AcademicYearId} not found. Error: {Error}", id, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Academic Year Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed while updating academic year {AcademicYearId}. Error: {Error}", 
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
            _logger.LogError(ex, "Unexpected error occurred while updating academic year {AcademicYearId}", id);
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
    /// Deletes a specific academic year
    /// </summary>
    /// <param name="institutionId">The institution ID</param>
    /// <param name="id">The academic year ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Academic year deleted successfully</response>
    /// <response code="400">Invalid request or academic year has dependencies</response>
    /// <response code="404">Academic year not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(Guid institutionId, Guid id)
    {
        _logger.LogInformation("Attempting to delete academic year {AcademicYearId} for institution {InstitutionId}", 
            id, institutionId);

        try
        {
            // Get the existing academic year first to verify it belongs to the specified institution
            var existingAcademicYear = await _academicYearService.GetByIdAsync(id);
            if (existingAcademicYear.InstitutionId != institutionId)
            {
                _logger.LogWarning("Academic year {AcademicYearId} does not belong to institution {InstitutionId}", 
                    id, institutionId);
                return BadRequest(new ProblemDetails
                {
                    Title = "Institution Mismatch",
                    Detail = $"Academic year with ID {id} does not belong to institution with ID {institutionId}",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                });
            }

            await _academicYearService.DeleteAsync(id);
            
            _logger.LogInformation("Successfully deleted academic year {AcademicYearId} for institution {InstitutionId}", 
                id, institutionId);
            
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Academic year {AcademicYearId} not found. Error: {Error}", id, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Academic Year Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Cannot delete academic year {AcademicYearId} due to dependencies. Error: {Error}", 
                id, ex.Message);
            return BadRequest(new ProblemDetails
            {
                Title = "Cannot Delete Academic Year",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting academic year {AcademicYearId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while processing your request.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = HttpContext.Request.Path
            });
        }
    }

    #region Additional endpoints for academic year utilities

    /// <summary>
    /// Gets all academic years without pagination (for dropdowns, calculations)
    /// </summary>
    /// <param name="institutionId">The institution ID</param>
    /// <returns>List of academic years</returns>
    [HttpGet("all")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    [ProducesResponseType(typeof(IEnumerable<AcademicYearResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AcademicYearResponseDto>>> GetAllNonPaginated(Guid institutionId)
    {
        try
        {
            var academicYears = await _academicYearService.GetAllByInstitutionAsync(institutionId);
            return Ok(academicYears);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all academic years (non-paginated) for institution {InstitutionId}", institutionId);
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
    /// Gets the current active academic year for an institution
    /// </summary>
    /// <param name="institutionId">The institution ID</param>
    /// <returns>Current academic year</returns>
    [HttpGet("current")]
    [Authorize(Roles = "Admin,SuperAdmin,Student,Teacher,Parent")]
    [ProducesResponseType(typeof(AcademicYearResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AcademicYearResponseDto>> GetCurrent(Guid institutionId)
    {
        try
        {
            var currentAcademicYear = await _academicYearService.GetCurrentAsync(institutionId);
            return Ok(currentAcademicYear);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("No current academic year found for institution {InstitutionId}. Error: {Error}", 
                institutionId, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Current Academic Year Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current academic year for institution {InstitutionId}", institutionId);
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
    /// Gets all active academic years for an institution
    /// </summary>
    /// <param name="institutionId">The institution ID</param>
    /// <returns>List of active academic years</returns>
    [HttpGet("active")]
    [Authorize(Roles = "Admin,SuperAdmin,Student,Teacher,Parent")]
    [ProducesResponseType(typeof(IEnumerable<AcademicYearResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AcademicYearResponseDto>>> GetActive(Guid institutionId)
    {
        try
        {
            var activeAcademicYears = await _academicYearService.GetActiveAsync(institutionId);
            return Ok(activeAcademicYears);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active academic years for institution {InstitutionId}", institutionId);
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