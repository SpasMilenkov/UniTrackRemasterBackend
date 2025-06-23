using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Services.Organization;
using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;

namespace UniTrackRemaster.Controllers;

/// <summary>
/// Controller for managing educational institutions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InstitutionsController : ControllerBase
{
    private readonly IInstitutionService _institutionService;
    private readonly ILogger<InstitutionsController> _logger;

    public InstitutionsController(IInstitutionService institutionService, ILogger<InstitutionsController> logger)
    {
        _institutionService = institutionService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all institutions (legacy endpoint - consider using paginated version for large datasets)
    /// </summary>
    /// <returns>List of all institutions</returns>
    /// <response code="200">Institutions retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("all")]
    [ProducesResponseType(typeof(IEnumerable<InstitutionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<InstitutionDto>>> GetAll()
    {
        _logger.LogInformation("Attempting to retrieve all institutions (legacy endpoint)");
        
        try
        {
            var institutions = await _institutionService.GetAllAsync();
            _logger.LogInformation("Successfully retrieved {Count} institutions", institutions.Count);
            return Ok(institutions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving all institutions");
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
    /// Gets all institutions with optional filtering and pagination
    /// </summary>
    /// <param name="name">Optional name filter (case-insensitive partial match)</param>
    /// <param name="type">Optional institution type filter</param>
    /// <param name="location">Optional location type filter</param>
    /// <param name="integrationStatus">Optional integration status filter</param>
    /// <param name="accreditations">Optional accreditations filter (comma-separated list)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <returns>Paginated list of institutions matching the filters</returns>
    /// <response code="200">Institutions retrieved successfully</response>
    /// <response code="400">Invalid query parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<InstitutionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<InstitutionDto>>> GetAllPaginated(
        [FromQuery] string? name = null,
        [FromQuery] string? type = null,
        [FromQuery] string? location = null,
        [FromQuery] string? integrationStatus = null,
        [FromQuery] string? accreditations = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 50)
    {
        _logger.LogInformation("Attempting to retrieve institutions with filters - Name: {Name}, Type: {Type}, Location: {Location}, IntegrationStatus: {IntegrationStatus}, Accreditations: {Accreditations}, Page: {Page}, PageSize: {PageSize}", 
            name, type, location, integrationStatus, accreditations, page, pageSize);
        
        try
        {
            var institutions = await _institutionService.GetAllAsync(name, type, location, integrationStatus, accreditations, page, pageSize);
            _logger.LogInformation("Successfully retrieved {Count} institutions (Page {Page} of {TotalPages})", 
                institutions.Items.Count, institutions.CurrentPage, institutions.TotalPages);
            return Ok(institutions);
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving institutions");
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
    /// Gets an institution by ID
    /// </summary>
    /// <param name="id">Institution GUID</param>
    /// <returns>Institution details</returns>
    /// <response code="200">Institution found and returned successfully</response>
    /// <response code="404">Institution not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InstitutionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InstitutionDto>> GetById(Guid id)
    {
        _logger.LogInformation("Attempting to retrieve institution with ID: {InstitutionId}", id);
        
        try
        {
            var institution = await _institutionService.GetByIdAsync(id);
            _logger.LogInformation("Successfully retrieved institution with ID: {InstitutionId}", id);
            return Ok(institution);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Institution not found with ID: {InstitutionId}. Error: {Error}", id, ex.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving institution with ID: {InstitutionId}", id);
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
    /// Updates an institution
    /// </summary>
    /// <param name="id">Institution GUID</param>
    /// <param name="updateDto">Updated institution data</param>
    /// <param name="logo">Optional new logo file</param>
    /// <param name="newImages">Optional new image files</param>
    /// <returns>Success status</returns>
    /// <response code="204">Institution updated successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="404">Institution not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateInstitutionDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for institution update. ID: {InstitutionId}. ValidationErrors: {@ValidationErrors}", 
                id, ModelState.Where(x => x.Value.Errors.Count > 0));
            return BadRequest(new ValidationProblemDetails(ModelState)
            {
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }
    
        _logger.LogInformation("Attempting to update institution with ID: {InstitutionId}", id);
        
        try
        {
            await _institutionService.UpdateAsync(id, updateDto);
            _logger.LogInformation("Successfully updated institution with ID: {InstitutionId}", id);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Institution not found for update. ID: {InstitutionId}. Error: {Error}", id, ex.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while updating institution with ID: {InstitutionId}", id);
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
    /// Gets institutions associated with a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of institutions associated with the user</returns>
    /// <response code="200">User institutions retrieved successfully</response>
    /// <response code="403">Access denied - user can only access their own institutions</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(List<InstitutionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<InstitutionDto>>> GetUserInstitutions(string userId)
    {
        _logger.LogInformation("Attempting to retrieve institutions for user: {UserId}", userId);
        
        // Verify the requesting user can only access their own institutions
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId != userId)
        {
            _logger.LogWarning("Access denied. User {CurrentUserId} attempted to access institutions for user {RequestedUserId}", 
                currentUserId, userId);
            return new ObjectResult(new ProblemDetails
            {
                Title = "Access Denied",
                Detail = "You can only access your own institutions.",
                Status = StatusCodes.Status403Forbidden,
                Instance = HttpContext.Request.Path
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };

        }

        try
        {
            var institutions = await _institutionService.GetInstitutionsByUserId(userId);
            _logger.LogInformation("Successfully retrieved {Count} institutions for user: {UserId}", institutions.Count, userId);
            return Ok(institutions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving institutions for user: {UserId}", userId);
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
    /// Deletes an institution
    /// </summary>
    /// <param name="id">Institution GUID</param>
    /// <returns>Success status</returns>
    /// <response code="204">Institution deleted successfully</response>
    /// <response code="404">Institution not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Attempting to delete institution with ID: {InstitutionId}", id);
        
        try
        {
            await _institutionService.DeleteAsync(id);
            _logger.LogInformation("Successfully deleted institution with ID: {InstitutionId}", id);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Institution not found for deletion. ID: {InstitutionId}. Error: {Error}", id, ex.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while deleting institution with ID: {InstitutionId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while processing your request.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = HttpContext.Request.Path
            });
        }
    }
}