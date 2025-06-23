using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Parent;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Services.User;
using UniTrackRemaster.Services.User.Parents;
using UniTrackRemaster.Services.User.Parents.Exceptions;

namespace UniTrackRemaster.Controllers;

/// <summary>
/// Controller for managing parent profiles and relationships
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ParentsController : ControllerBase
{
    private readonly IParentService _service;
    private readonly ILogger<ParentsController> _logger;

    public ParentsController(IParentService service, ILogger<ParentsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Gets a parent by their ID
    /// </summary>
    /// <param name="id">Parent GUID</param>
    /// <returns>Parent details with children information</returns>
    /// <response code="200">Parent found and returned successfully</response>
    /// <response code="404">Parent not found</response>
    /// <response code="422">Business rule violation (e.g., data integrity issues)</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ParentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParentResponseDto>> GetById(Guid id)
    {
        _logger.LogInformation("Attempting to retrieve parent with ID: {ParentId}", id);
        
        try
        {
            var parent = await _service.GetByIdAsync(id);
            _logger.LogInformation("Successfully retrieved parent with ID: {ParentId}", id);
            return Ok(parent);
        }
        catch (ParentNotFoundException ex)
        {
            _logger.LogWarning("Parent not found with ID: {ParentId}. Error: {Error}", id, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Parent Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (ParentBusinessRuleViolationException ex)
        {
            _logger.LogError("Business rule violation retrieving parent with ID: {ParentId}. Error: {Error}", id, ex.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving parent with ID: {ParentId}", id);
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
    /// Gets a parent by their user ID
    /// </summary>
    /// <param name="userId">User GUID</param>
    /// <returns>Parent details for the specified user</returns>
    /// <response code="200">Parent found and returned successfully</response>
    /// <response code="404">Parent not found for user</response>
    /// <response code="422">Business rule violation (e.g., data integrity issues)</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(ParentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParentResponseDto>> GetByUserId(Guid userId)
    {
        _logger.LogInformation("Attempting to retrieve parent for user ID: {UserId}", userId);
        
        try
        {
            var parent = await _service.GetByUserIdAsync(userId);
            return Ok(parent);
        }
        catch (ParentNotFoundException ex)
        {
            _logger.LogWarning("Parent not found for user ID: {UserId}. Error: {Error}", userId, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Parent Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (ParentBusinessRuleViolationException ex)
        {
            _logger.LogError("Business rule violation retrieving parent for user ID: {UserId}. Error: {Error}", userId, ex.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving parent for user ID: {UserId}", userId);
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
    /// Gets all parents with optional filtering and pagination
    /// </summary>
    /// <param name="status">Optional status filter (Pending, Active, Inactive, Suspended, Rejected)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <returns>Paginated list of all parents</returns>
    /// <response code="200">Parents retrieved successfully</response>
    /// <response code="400">Invalid query parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ParentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<ParentResponseDto>>> GetAll(
        [FromQuery] string? status = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 50)
    {
        _logger.LogInformation("Attempting to retrieve parents with filters - Status: {Status}, Page: {Page}, PageSize: {PageSize}", 
            status, page, pageSize);
        
        try
        {
            var parents = await _service.GetAllAsync(status, page, pageSize);
            _logger.LogInformation("Successfully retrieved {Count} parents (Page {Page} of {TotalPages})", 
                parents.Items.Count, parents.CurrentPage, parents.TotalPages);
            return Ok(parents);
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving parents");
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
    /// Gets children of a specific parent
    /// </summary>
    /// <param name="parentId">Parent GUID</param>
    /// <returns>List of parent's children</returns>
    /// <response code="200">Children retrieved successfully</response>
    /// <response code="404">Parent not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{parentId:guid}/children")]
    [ProducesResponseType(typeof(List<ChildResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ChildResponseDto>>> GetChildren(Guid parentId)
    {
        _logger.LogInformation("Attempting to retrieve children for parent: {ParentId}", parentId);
        
        try
        {
            var children = await _service.GetChildrenAsync(parentId);
            _logger.LogInformation("Successfully retrieved {Count} children for parent: {ParentId}", 
                children.Count, parentId);
            return Ok(children);
        }
        catch (ParentNotFoundException ex)
        {
            _logger.LogWarning("Parent not found: {ParentId}. Error: {Error}", parentId, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Parent Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving children for parent: {ParentId}", parentId);
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
    /// Creates a new parent profile
    /// </summary>
    /// <param name="dto">Parent creation data</param>
    /// <returns>Created parent details</returns>
    /// <response code="201">Parent created successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="409">Conflict - parent already exists for user</response>
    /// <response code="422">Unprocessable entity - business rule violation</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(ParentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParentResponseDto>> Create([FromBody] CreateParentDto dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for parent creation: {@ValidationErrors}", 
                ModelState.Where(x => x.Value.Errors.Count > 0));
            return BadRequest(new ValidationProblemDetails(ModelState)
            {
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }

        _logger.LogInformation("Attempting to create parent for user: {UserId}", dto.UserId);
        
        try
        {
            var createdParent = await _service.CreateAsync(dto);
            _logger.LogInformation("Successfully created parent with ID: {ParentId}", createdParent.Id);
            
            return CreatedAtAction(
                nameof(GetById), 
                new { id = createdParent.Id }, 
                createdParent);
        }
        catch (DuplicateParentException ex)
        {
            _logger.LogWarning("Duplicate parent creation attempt. Error: {Error}", ex.Message);
            return Conflict(new ProblemDetails
            {
                Title = "Parent Already Exists",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            });
        }
        catch (ParentBusinessRuleViolationException ex)
        {
            _logger.LogWarning("Business rule violation during parent creation. Error: {Error}", ex.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while creating parent for user: {UserId}", dto.UserId);
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
    /// Updates parent profile information
    /// </summary>
    /// <param name="id">Parent GUID</param>
    /// <param name="dto">Updated parent data</param>
    /// <returns>Updated parent details</returns>
    /// <response code="200">Parent updated successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="404">Parent not found</response>
    /// <response code="409">Parent cannot be updated in current state</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ParentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParentResponseDto>> Update(Guid id, [FromBody] UpdateParentDto dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for parent update. ID: {ParentId}. ValidationErrors: {@ValidationErrors}", 
                id, ModelState.Where(x => x.Value.Errors.Count > 0));
            return BadRequest(new ValidationProblemDetails(ModelState)
            {
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }

        _logger.LogInformation("Attempting to update parent with ID: {ParentId}", id);
        
        try
        {
            var updated = await _service.UpdateAsync(id, dto);
            _logger.LogInformation("Successfully updated parent with ID: {ParentId}", id);
            return Ok(updated);
        }
        catch (ParentNotFoundException ex)
        {
            _logger.LogWarning("Parent not found for update. ID: {ParentId}. Error: {Error}", id, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Parent Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidParentStateException ex)
        {
            _logger.LogWarning("Parent cannot be updated in current state. ID: {ParentId}. Error: {Error}", 
                id, ex.Message);
            return Conflict(new ProblemDetails
            {
                Title = "Invalid Parent State",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating parent with ID: {ParentId}", id);
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
    /// Updates parent status
    /// </summary>
    /// <param name="id">Parent GUID</param>
    /// <param name="status">New status (Pending, Active, Inactive, Suspended, Rejected)</param>
    /// <returns>Updated parent details</returns>
    /// <response code="200">Parent status updated successfully</response>
    /// <response code="400">Invalid status value</response>
    /// <response code="404">Parent not found</response>
    /// <response code="409">Status change not allowed from current state</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(ParentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParentResponseDto>> UpdateStatus(Guid id, [FromBody] ProfileStatus status)
    {
        _logger.LogInformation("Attempting to update parent status: {ParentId} to {Status}", id, status);
        
        try
        {
            var updated = await _service.UpdateStatusAsync(id, status);
            _logger.LogInformation("Successfully updated parent status: {ParentId} to {Status}", id, status);
            return Ok(updated);
        }
        catch (ParentNotFoundException ex)
        {
            _logger.LogWarning("Parent not found for status update. ID: {ParentId}. Error: {Error}", id, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Parent Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidParentStateException ex)
        {
            _logger.LogWarning("Invalid status transition for parent. ID: {ParentId}. Error: {Error}", id, ex.Message);
            return Conflict(new ProblemDetails
            {
                Title = "Invalid Status Transition",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating parent status: {ParentId}", id);
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
    /// Activates a parent profile
    /// </summary>
    /// <param name="id">Parent GUID</param>
    /// <returns>Activated parent details</returns>
    /// <response code="200">Parent activated successfully</response>
    /// <response code="404">Parent not found</response>
    /// <response code="409">Parent cannot be activated from current state</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:guid}/activate")]
    [ProducesResponseType(typeof(ParentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParentResponseDto>> Activate(Guid id)
    {
        _logger.LogInformation("Attempting to activate parent: {ParentId}", id);
        
        try
        {
            var activated = await _service.ActivateAsync(id);
            _logger.LogInformation("Successfully activated parent: {ParentId}", id);
            return Ok(activated);
        }
        catch (ParentNotFoundException ex)
        {
            _logger.LogWarning("Parent not found for activation. ID: {ParentId}. Error: {Error}", id, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Parent Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidParentStateException ex)
        {
            _logger.LogWarning("Parent cannot be activated from current state. ID: {ParentId}. Error: {Error}", 
                id, ex.Message);
            return Conflict(new ProblemDetails
            {
                Title = "Invalid Parent State",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while activating parent: {ParentId}", id);
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
    /// Suspends a parent profile
    /// </summary>
    /// <param name="id">Parent GUID</param>
    /// <returns>Suspended parent details</returns>
    /// <response code="200">Parent suspended successfully</response>
    /// <response code="404">Parent not found</response>
    /// <response code="409">Parent cannot be suspended from current state</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:guid}/suspend")]
    [ProducesResponseType(typeof(ParentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParentResponseDto>> Suspend(Guid id)
    {
        _logger.LogInformation("Attempting to suspend parent: {ParentId}", id);
        
        try
        {
            var suspended = await _service.SuspendAsync(id);
            _logger.LogInformation("Successfully suspended parent: {ParentId}", id);
            return Ok(suspended);
        }
        catch (ParentNotFoundException ex)
        {
            _logger.LogWarning("Parent not found for suspension. ID: {ParentId}. Error: {Error}", id, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Parent Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (InvalidParentStateException ex)
        {
            _logger.LogWarning("Parent cannot be suspended from current state. ID: {ParentId}. Error: {Error}", 
                id, ex.Message);
            return Conflict(new ProblemDetails
            {
                Title = "Invalid Parent State",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while suspending parent: {ParentId}", id);
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
    /// Adds a child to a parent
    /// </summary>
    /// <param name="parentId">Parent GUID</param>
    /// <param name="dto">Child addition data</param>
    /// <returns>Updated parent details with new child</returns>
    /// <response code="200">Child added successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="404">Parent or student not found</response>
    /// <response code="409">Conflict - child already associated or business rule violation</response>
    /// <response code="422">Unprocessable entity - business rule violation</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{parentId:guid}/children")]
    [ProducesResponseType(typeof(ParentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParentResponseDto>> AddChild(Guid parentId, [FromBody] AddChildDto dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for adding child. Parent: {ParentId}, Student: {StudentId}. ValidationErrors: {@ValidationErrors}", 
                parentId, dto.StudentId, ModelState.Where(x => x.Value.Errors.Count > 0));
            return BadRequest(new ValidationProblemDetails(ModelState)
            {
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }

        _logger.LogInformation("Attempting to add child {StudentId} to parent: {ParentId}", dto.StudentId, parentId);
        
        try
        {
            var updated = await _service.AddChildAsync(parentId, dto);
            _logger.LogInformation("Successfully added child {StudentId} to parent: {ParentId}", dto.StudentId, parentId);
            return Ok(updated);
        }
        catch (ParentNotFoundException ex)
        {
            _logger.LogWarning("Parent not found for adding child. ID: {ParentId}. Error: {Error}", parentId, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Parent Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (DuplicateChildException ex)
        {
            _logger.LogWarning("Child already associated with parent. Parent: {ParentId}, Student: {StudentId}. Error: {Error}", 
                parentId, dto.StudentId, ex.Message);
            return Conflict(new ProblemDetails
            {
                Title = "Child Already Associated",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            });
        }
        catch (MaxParentsExceededException ex)
        {
            _logger.LogWarning("Student has maximum number of parents. Student: {StudentId}. Error: {Error}", 
                dto.StudentId, ex.Message);
            return Conflict(new ProblemDetails
            {
                Title = "Maximum Parents Exceeded",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            });
        }
        catch (ParentBusinessRuleViolationException ex)
        {
            _logger.LogWarning("Business rule violation adding child. Parent: {ParentId}, Student: {StudentId}. Error: {Error}", 
                parentId, dto.StudentId, ex.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while adding child {StudentId} to parent: {ParentId}", 
                dto.StudentId, parentId);
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
    /// Removes a child from a parent
    /// </summary>
    /// <param name="parentId">Parent GUID</param>
    /// <param name="studentId">Student GUID to remove</param>
    /// <returns>Updated parent details without the removed child</returns>
    /// <response code="200">Child removed successfully</response>
    /// <response code="404">Parent or child association not found</response>
    /// <response code="409">Child is not associated with parent</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{parentId:guid}/children/{studentId:guid}")]
    [ProducesResponseType(typeof(ParentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParentResponseDto>> RemoveChild(Guid parentId, Guid studentId)
    {
        _logger.LogInformation("Attempting to remove child {StudentId} from parent: {ParentId}", studentId, parentId);
        
        try
        {
            var updated = await _service.RemoveChildAsync(parentId, studentId);
            _logger.LogInformation("Successfully removed child {StudentId} from parent: {ParentId}", studentId, parentId);
            return Ok(updated);
        }
        catch (ParentNotFoundException ex)
        {
            _logger.LogWarning("Parent not found for removing child. ID: {ParentId}. Error: {Error}", parentId, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Parent Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (ChildNotAssociatedException ex)
        {
            _logger.LogWarning("Child not associated with parent. Parent: {ParentId}, Student: {StudentId}. Error: {Error}", 
                parentId, studentId, ex.Message);
            return Conflict(new ProblemDetails
            {
                Title = "Child Not Associated",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while removing child {StudentId} from parent: {ParentId}", 
                studentId, parentId);
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
    /// Deletes a parent profile (soft delete)
    /// </summary>
    /// <param name="id">Parent GUID</param>
    /// <returns>Success status</returns>
    /// <response code="204">Parent deleted successfully</response>
    /// <response code="404">Parent not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Attempting to delete parent with ID: {ParentId}", id);
        
        try
        {
            var result = await _service.DeleteAsync(id);
            if (result)
            {
                _logger.LogInformation("Successfully deleted parent with ID: {ParentId}", id);
                return NoContent();
            }
            else
            {
                _logger.LogWarning("Parent not found for deletion. ID: {ParentId}", id);
                return NotFound(new ProblemDetails
                {
                    Title = "Parent Not Found",
                    Detail = "The specified parent could not be found.",
                    Status = StatusCodes.Status404NotFound,
                    Instance = HttpContext.Request.Path
                });
            }
        }
        catch (ParentNotFoundException ex)
        {
            _logger.LogWarning("Parent not found for deletion. ID: {ParentId}. Error: {Error}", id, ex.Message);
            return NotFound(new ProblemDetails
            {
                Title = "Parent Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting parent with ID: {ParentId}", id);
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