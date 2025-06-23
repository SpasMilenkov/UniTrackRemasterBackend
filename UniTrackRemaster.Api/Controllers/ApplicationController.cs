using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Services.Organization;
using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Api.Dto.Application;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Services.Organization.Exceptions.Application;
using NotFoundException = UniTrackRemaster.Data.Exceptions.NotFoundException;

namespace UniTrackRemaster.Controllers;

/// <summary>
/// Controller for managing institution applications
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _service;
    private readonly ILogger<ApplicationsController> _logger;

    public ApplicationsController(IApplicationService service, ILogger<ApplicationsController> logger)
    {
        _service = service;
        _logger = logger;
    }
    
    /// <summary>
    /// Gets an application by ID
    /// </summary>
    /// <param name="id">Application GUID</param>
    /// <returns>Application details</returns>
    /// <response code="200">Application found and returned successfully</response>
    /// <response code="404">Application not found</response>
    /// <response code="422">Business rule violation (e.g., data integrity issues)</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApplicationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApplicationResponseDto>> GetById(Guid id)
    {
        _logger.LogInformation("Attempting to retrieve application with ID: {ApplicationId}", id);
        
        try
        {
            var application = await _service.GetByIdAsync(id);
            _logger.LogInformation("Successfully retrieved application with ID: {ApplicationId}", id);
            return Ok(application);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Application not found with ID: {ApplicationId}. Error: {Error}", id, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Application Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogError("Business rule violation retrieving application with ID: {ApplicationId}. Error: {Error}", id, ex.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving application with ID: {ApplicationId}", id);
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
    /// Gets application for a specific institution
    /// </summary>
    /// <param name="institutionId">Institution GUID</param>
    /// <returns>Institution's application</returns>
    /// <response code="200">Application found and returned successfully</response>
    /// <response code="404">Institution not found or has no application</response>
    /// <response code="422">Business rule violation (e.g., data integrity issues)</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("institution/{institutionId:guid}")]
    [ProducesResponseType(typeof(ApplicationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApplicationResponseDto>> GetByInstitutionId(Guid institutionId)
    {
        _logger.LogInformation("Attempting to retrieve application for institution ID: {InstitutionId}", institutionId);
        
        try
        {
            var application = await _service.GetByInstitutionIdAsync(institutionId);
            return Ok(application);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Institution application not found. Institution ID: {InstitutionId}. Error: {Error}", 
                institutionId, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Institution Application Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogError("Business rule violation retrieving application for institution ID: {InstitutionId}. Error: {Error}", 
                institutionId, ex.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving application for institution ID: {InstitutionId}", 
                institutionId);
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
    /// Gets all applications with optional filtering and pagination
    /// </summary>
    /// <param name="status">Optional status filter</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <returns>Paginated list of all applications</returns>
    /// <response code="200">Applications retrieved successfully</response>
    /// <response code="400">Invalid query parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ApplicationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<ApplicationResponseDto>>> GetAll(
        [FromQuery] string? status = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 50)
    {
        _logger.LogInformation("Attempting to retrieve applications with filters - Status: {Status}, Page: {Page}, PageSize: {PageSize}", 
            status, page, pageSize);
        
        try
        {
            var applications = await _service.GetAllAsync(status, page, pageSize);
            _logger.LogInformation("Successfully retrieved {Count} applications (Page {Page} of {TotalPages})", 
                applications.Items.Count, applications.CurrentPage, applications.TotalPages);
            return Ok(applications);
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving applications");
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
    /// Creates a new institution application
    /// </summary>
    /// <param name="dto">Application creation data</param>
    /// <returns>Created application details</returns>
    /// <response code="201">Application created successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="409">Conflict - application already exists</response>
    /// <response code="422">Unprocessable entity - business rule violation</response>
    /// <response code="502">Bad Gateway - email delivery failed</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApplicationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApplicationResponseDto>> Create([FromBody] CreateInstitutionApplicationDto dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for application creation: {@ValidationErrors}", 
                ModelState.Where(x => x.Value.Errors.Count > 0));
            return BadRequest(new ValidationProblemDetails(ModelState)
            {
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }

        _logger.LogInformation("Attempting to create application for institution: {InstitutionName}", dto.InstitutionName);
        
        try
        {
            var createdApplication = await _service.CreateAsync(dto);
            _logger.LogInformation("Successfully created application with ID: {ApplicationId}", createdApplication.Id);
            
            return CreatedAtAction(
                nameof(GetById), 
                new { id = createdApplication.Id }, 
                createdApplication);
        }
        catch (DuplicateApplicationException ex)
        {
            _logger.LogWarning("Duplicate application creation attempt. Error: {Error}", ex.Message);
            return Conflict(new ProblemDetails
            {
                Title = "Application Already Exists",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            });
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogWarning("Business rule violation during application creation. Error: {Error}", ex.Message);
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Business Rule Violation",
                Detail = ex.Message,
                Status = StatusCodes.Status422UnprocessableEntity,
                Instance = HttpContext.Request.Path
            });
        }
        catch (EmailDeliveryException ex)
        {
            _logger.LogWarning("Email delivery failed during application creation. Error: {Error}", ex.Message);
            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Title = "Email Delivery Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status502BadGateway,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating application for institution: {InstitutionName}", 
                dto.InstitutionName);
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
    /// Verifies application using code and email
    /// </summary>
    /// <param name="request">Verification request containing code and email</param>
    /// <returns>Verified application details</returns>
    /// <response code="200">Code verified successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Invalid verification code</response>
    /// <response code="404">Application not found</response>
    /// <response code="422">Business rule violation (e.g., data integrity issues)</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("verify-code")]
    [ProducesResponseType(typeof(ApplicationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApplicationResponseDto>> VerifyCode([FromBody] VerifyCodeRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for code verification: {@ValidationErrors}", 
                ModelState.Where(x => x.Value.Errors.Count > 0));
            return BadRequest(new ValidationProblemDetails(ModelState)
            {
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }

        _logger.LogInformation("Attempting to verify code for email: {Email}", request.Email);
        
        try
        {
            var application = await _service.GetByCodeAsync(request.Code, request.Email);
            _logger.LogInformation("Successfully verified code for application ID: {ApplicationId}", application.Id);
            return Ok(application);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Application not found during code verification. Email: {Email}. Error: {Error}", 
                request.Email, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Application Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidVerificationCodeException ex)
        {
            _logger.LogWarning("Invalid verification code provided. Email: {Email}. Error: {Error}", 
                request.Email, ex.Message);
            return Unauthorized(new ProblemDetails
            {
                Title = "Invalid Verification Code",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized,
                Instance = HttpContext.Request.Path
            });
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogError("Business rule violation during code verification. Email: {Email}. Error: {Error}", 
                request.Email, ex.Message);
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
            _logger.LogError(ex, "Unexpected error occurred during code verification for email: {Email}", request.Email);
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
    /// Approves an application
    /// </summary>
    /// <param name="id">Application GUID</param>
    /// <returns>Success status</returns>
    /// <response code="204">Application approved successfully</response>
    /// <response code="404">Application not found</response>
    /// <response code="409">Application cannot be approved in current state</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("approve/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Approve(Guid id)
    {
        _logger.LogInformation("Attempting to approve application with ID: {ApplicationId}", id);
        
        try
        {
            await _service.ApproveAsync(id);
            _logger.LogInformation("Successfully approved application with ID: {ApplicationId}", id);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Application not found for approval. ID: {ApplicationId}. Error: {Error}", id, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Application Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidApplicationStateException ex)
        {
            _logger.LogWarning("Application cannot be approved in current state. ID: {ApplicationId}. Error: {Error}", 
                id, ex.Message);
            return Conflict(new ProblemDetails
            {
                Title = "Invalid Application State",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while approving application with ID: {ApplicationId}", id);
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
    /// Updates application details
    /// </summary>
    /// <param name="id">Application GUID</param>
    /// <param name="dto">Updated application data</param>
    /// <returns>Updated application details</returns>
    /// <response code="200">Application updated successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="404">Application not found</response>
    /// <response code="409">Application cannot be updated in current state</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApplicationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApplicationResponseDto>> Update(Guid id, [FromBody] UpdateInstitutionApplicationDto dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for application update. ID: {ApplicationId}. ValidationErrors: {@ValidationErrors}", 
                id, ModelState.Where(x => x.Value.Errors.Count > 0));
            return BadRequest(new ValidationProblemDetails(ModelState)
            {
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }

        _logger.LogInformation("Attempting to update application with ID: {ApplicationId}", id);
        
        try
        {
            var updated = await _service.UpdateAsync(id, dto);
            _logger.LogInformation("Successfully updated application with ID: {ApplicationId}", id);
            return Ok(updated);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Application not found for update. ID: {ApplicationId}. Error: {Error}", id, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Application Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidApplicationStateException ex)
        {
            _logger.LogWarning("Application cannot be updated in current state. ID: {ApplicationId}. Error: {Error}", 
                id, ex.Message);
            return Conflict(new ProblemDetails
            {
                Title = "Invalid Application State",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating application with ID: {ApplicationId}", id);
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
    /// Deletes an application
    /// </summary>
    /// <param name="id">Application GUID</param>
    /// <returns>Success status</returns>
    /// <response code="204">Application deleted successfully</response>
    /// <response code="404">Application not found</response>
    /// <response code="409">Application cannot be deleted in current state</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Attempting to delete application with ID: {ApplicationId}", id);
        
        try
        {
            var result = await _service.DeleteAsync(id);
            if (result)
            {
                _logger.LogInformation("Successfully deleted application with ID: {ApplicationId}", id);
                return NoContent();
            }
            else
            {
                _logger.LogWarning("Application not found for deletion. ID: {ApplicationId}", id);
                return NotFound(new ProblemDetails
                {
                    Title = "Application Not Found",
                    Detail = "The specified application could not be found.",
                    Status = StatusCodes.Status404NotFound,
                    Instance = HttpContext.Request.Path
                });
            }
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Application not found for deletion. ID: {ApplicationId}. Error: {Error}", id, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Application Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidApplicationStateException ex)
        {
            _logger.LogWarning("Application cannot be deleted in current state. ID: {ApplicationId}. Error: {Error}", 
                id, ex.Message);
            return Conflict(new ProblemDetails
            {
                Title = "Invalid Application State",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting application with ID: {ApplicationId}", id);
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