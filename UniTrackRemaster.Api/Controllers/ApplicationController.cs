using Microsoft.AspNetCore.Mvc;
using OrganizationServices;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Controllers;

/// <summary>
/// Controller for managing institution applications
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _service;

    public ApplicationsController(IApplicationService service)
    {
        _service = service;
    }
    
    /// <summary>
    /// Gets an application by ID
    /// </summary>
    /// <param name="id">Application GUID</param>
    /// <returns>Application details or 404 if not found</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApplicationResponseDto>> GetById(Guid id)
    {
        try
        {
            var application = await _service.GetByIdAsync(id);
            return Ok(application);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    /// <summary>
    /// Gets applications for a specific institution
    /// </summary>
    /// <param name="institutionId">Institution GUID</param>
    /// <returns>Institution's applications or 404</returns>
    [HttpGet("institution/{institutionId:guid}")]
    public async Task<ActionResult<ApplicationResponseDto>> GetByInstitutionId(Guid institutionId)
    {
        try
        {
            var application = await _service.GetByInstitutionIdAsync(institutionId);
            return Ok(application);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Gets all applications
    /// </summary>
    /// <returns>List of all applications</returns>
    [HttpGet]
    public async Task<ActionResult<List<ApplicationResponseDto>>> GetAll()
    {
        var applications = await _service.GetAllAsync();
        return Ok(applications);
    }

    /// <summary>
    /// Creates new institution application
    /// </summary>
    /// <param name="dto">Application creation data</param>
    /// <returns>Created application details or 400 if invalid</returns>    
    [HttpPost]
    public async Task<ActionResult<ApplicationResponseDto>> Create(CreateInstitutionApplicationDto dto)
    {
        try
        {
            var createdApplication = await _service.CreateAsync(dto);
            return CreatedAtAction(
                nameof(GetById), 
                new { id = createdApplication.Id }, 
                createdApplication);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    
    /// <summary>
    /// Verifies application code
    /// </summary>
    /// <param name="code">Verification code</param>
    /// <param name="email">Associated email</param>
    /// <returns>Verified application or error status</returns>
    [HttpPost("verify-code")]
    public async Task<ActionResult<ApplicationResponseDto>> VerifyCode(string code, string email)
    {
        try
        {
            var application = await _service.GetByCodeAsync(code, email);
            return Ok(application);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    /// <summary>
    /// Approves an application
    /// </summary>
    /// <param name="id">Application GUID</param>
    /// <returns>204 if successful, 404 if not found</returns>
    [HttpPut("approve/{id:guid}")]
    public async Task<IActionResult> Approve(Guid id)
    {
        try
        {
            await _service.ApproveAsync(id);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Updates application details
    /// </summary>
    /// <param name="id">Application GUID</param>
    /// <param name="dto">Updated application data</param>
    /// <returns>Updated application or 404</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateInstitutionApplicationDto dto)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, dto);
            return Ok(updated);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Deletes an application
    /// </summary>
    /// <param name="id">Application GUID</param>
    /// <returns>204 if deleted, 404 if not found</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            return result ? NoContent() : NotFound();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}