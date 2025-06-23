using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Services.Organization;
using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Repositories;

namespace UniTrackRemaster.Controllers;

/// <summary>
/// Controller for managing schools
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SchoolController : ControllerBase
{
    private readonly ISchoolService _schoolService;
    private readonly ILogger<SchoolController> _logger;

    public SchoolController(ISchoolService schoolService, ILogger<SchoolController> logger)
    {
        _schoolService = schoolService;
        _logger = logger;
    }

    /// <summary>
    /// Initializes a new school from an approved institution application
    /// </summary>
    /// <param name="schoolData">School initialization data</param>
    /// <param name="files">Logo and additional images</param>
    /// <returns>Success message with school institution ID</returns>
    /// <response code="201">School initialized successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="404">Institution not found</response>
    /// <response code="409">Institution already initialized as school</response>
    /// <response code="422">Business rule violation (e.g., application not approved)</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("init")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InitSchool(
        [FromForm] InitSchoolDto schoolData,
        [FromForm] SchoolFilesModel files)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for school initialization. InstitutionId: {InstitutionId}. ValidationErrors: {@ValidationErrors}", 
                schoolData.Id, ModelState.Where(x => x.Value.Errors.Count > 0));
            return BadRequest(new ValidationProblemDetails(ModelState)
            {
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }

        _logger.LogInformation("Attempting to initialize school for institution: {InstitutionId}, Name: {Name}", 
            schoolData.Id, schoolData.Name);

        try
        {
            var institutionId = await _schoolService.InitSchoolAsync(schoolData, files.Logo, files.AdditionalImages);
            _logger.LogInformation("Successfully initialized school for institution: {InstitutionId}", institutionId);

            return CreatedAtAction(
                nameof(GetSchoolByInstitutionId),
                new { institutionId },
                new { 
                    Message = "School initialized successfully",
                    InstitutionId = institutionId
                });
        }
        catch (InstitutionNotFoundException ex)
        {
            _logger.LogWarning("Institution not found during school initialization. InstitutionId: {InstitutionId}. Error: {Error}", 
                schoolData.Id, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Institution Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InstitutionAlreadyInitializedException ex)
        {
            _logger.LogWarning("Institution already initialized as school. InstitutionId: {InstitutionId}. Error: {Error}", 
                schoolData.Id, ex.Message);
            return Conflict(new ProblemDetails
            {
                Title = "Institution Already Initialized",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Business rule violation during school initialization. InstitutionId: {InstitutionId}. Error: {Error}", 
                schoolData.Id, ex.Message);
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Business Rule Violation",
                Detail = ex.Message,
                Status = StatusCodes.Status422UnprocessableEntity,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while initializing school for institution: {InstitutionId}", 
                schoolData.Id);
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
    /// Gets all schools with optional filtering and pagination
    /// </summary>
    /// <param name="filter">Filter and pagination parameters</param>
    /// <returns>List of schools matching the filters</returns>
    /// <response code="200">Schools retrieved successfully</response>
    /// <response code="400">Invalid query parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<SchoolWithAddressResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<SchoolWithAddressResponseDto>>> GetSchools([FromQuery] SchoolFilterDto filter)
    {
        _logger.LogInformation("Attempting to retrieve schools with filters - SearchTerm: {SearchTerm}, Types: {Types}, Page: {Page}, PageSize: {PageSize}", 
            filter.SearchTerm, filter.Types != null ? string.Join(",", filter.Types) : "null", filter.Page, filter.PageSize);

        try
        {
            // Validate pagination parameters
            if (filter.Page < 1)
            {
                throw new ArgumentException("Page number must be greater than 0", nameof(filter.Page));
            }

            if (filter.PageSize < 1 || filter.PageSize > 100)
            {
                throw new ArgumentException("Page size must be between 1 and 100", nameof(filter.PageSize));
            }

            var schools = await _schoolService.GetSchoolsAsync(filter);
            _logger.LogInformation("Successfully retrieved {Count} schools", schools.Count);
            return Ok(schools);
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving schools");
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
    /// Gets a school by ID
    /// </summary>
    /// <param name="id">School GUID</param>
    /// <returns>School details</returns>
    /// <response code="200">School found and returned successfully</response>
    /// <response code="404">School not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SchoolWithAddressResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SchoolWithAddressResponseDto>> GetSchool(Guid id)
    {
        _logger.LogInformation("Attempting to retrieve school with ID: {SchoolId}", id);

        try
        {
            var school = await _schoolService.GetSchoolAsync(id);
            _logger.LogInformation("Successfully retrieved school with ID: {SchoolId}", id);
            return Ok(school);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("School not found with ID: {SchoolId}. Error: {Error}", id, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "School Not Found",
                Detail = $"School with ID {id} was not found.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving school with ID: {SchoolId}", id);
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
    /// Gets a school by institution ID
    /// </summary>
    /// <param name="institutionId">Institution GUID</param>
    /// <returns>School details</returns>
    /// <response code="200">School found and returned successfully</response>
    /// <response code="404">School not found for the given institution</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("institution/{institutionId:guid}")]
    [ProducesResponseType(typeof(SchoolWithAddressResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SchoolWithAddressResponseDto>> GetSchoolByInstitutionId(Guid institutionId)
    {
        _logger.LogInformation("Attempting to retrieve school for institution ID: {InstitutionId}", institutionId);

        try
        {
            var school = await _schoolService.GetSchoolByInstitutionIdAsync(institutionId);
            _logger.LogInformation("Successfully retrieved school for institution ID: {InstitutionId}", institutionId);
            return Ok(school);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("School not found for institution ID: {InstitutionId}. Error: {Error}", institutionId, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "School Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving school for institution ID: {InstitutionId}", institutionId);
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
    /// Updates a school
    /// </summary>
    /// <param name="updateDto">Updated school data</param>
    /// <returns>Updated school details</returns>
    /// <response code="200">School updated successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="404">School not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut]
    [ProducesResponseType(typeof(SchoolBaseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SchoolBaseResponseDto>> UpdateSchool([FromBody] UpdateSchoolDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for school update. SchoolId: {SchoolId}. ValidationErrors: {@ValidationErrors}", 
                updateDto.SchoolId, ModelState.Where(x => x.Value.Errors.Count > 0));
            return BadRequest(new ValidationProblemDetails(ModelState)
            {
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }

        _logger.LogInformation("Attempting to update school with ID: {SchoolId}", updateDto.SchoolId);

        try
        {
            var school = await _schoolService.UpdateSchoolAsync(updateDto);
            _logger.LogInformation("Successfully updated school with ID: {SchoolId}", updateDto.SchoolId);
            return Ok(school);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("School not found for update. SchoolId: {SchoolId}. Error: {Error}", updateDto.SchoolId, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "School Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating school with ID: {SchoolId}", updateDto.SchoolId);
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
    /// Deletes a school
    /// </summary>
    /// <param name="id">School GUID</param>
    /// <returns>Success status</returns>
    /// <response code="204">School deleted successfully</response>
    /// <response code="404">School not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteSchool(Guid id)
    {
        _logger.LogInformation("Attempting to delete school with ID: {SchoolId}", id);

        try
        {
            await _schoolService.DeleteSchoolAsync(id);
            _logger.LogInformation("Successfully deleted school with ID: {SchoolId}", id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("School not found for deletion. SchoolId: {SchoolId}. Error: {Error}", id, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "School Not Found",
                Detail = $"School with ID {id} was not found.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting school with ID: {SchoolId}", id);
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
    /// Uploads additional images for a school
    /// </summary>
    /// <param name="schoolId">School GUID</param>
    /// <param name="images">Image files to upload</param>
    /// <returns>List of uploaded image URLs</returns>
    /// <response code="200">Images uploaded successfully</response>
    /// <response code="400">Invalid request or no images provided</response>
    /// <response code="404">School not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{schoolId:guid}/images")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<string>>> UploadSchoolImages(
        Guid schoolId, 
        [FromForm] List<IFormFile> images)
    {
        if (images == null || !images.Any())
        {
            _logger.LogWarning("No images provided for upload. SchoolId: {SchoolId}", schoolId);
            return BadRequest(new ProblemDetails
            {
                Title = "No Images Provided",
                Detail = "At least one image must be provided for upload.",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }

        _logger.LogInformation("Attempting to upload {ImageCount} images for school: {SchoolId}", images.Count, schoolId);

        try
        {
            var uploadedUrls = await _schoolService.UploadSchoolImagesAsync(schoolId, images);
            _logger.LogInformation("Successfully uploaded {ImageCount} images for school: {SchoolId}", uploadedUrls.Count, schoolId);
            return Ok(uploadedUrls);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("School not found for image upload. SchoolId: {SchoolId}. Error: {Error}", schoolId, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "School Not Found",
                Detail = $"School with ID {schoolId} was not found.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while uploading images for school: {SchoolId}", schoolId);
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