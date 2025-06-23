using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Services.Organization;
using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Api.Dto.University;
using UniTrackRemaster.Commons.Services;

namespace UniTrackRemaster.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UniversitiesController(IUniversityService universityService, ILogger<UniversityService> logger) : ControllerBase
{
    [HttpPost("init")]
    public async Task<IActionResult> InitUniversity(
        [FromForm] InitUniversityDto universityData,
        [FromForm] UniversityFilesModel files)
    {
        var universityId = await universityService.InitUniversityAsync(universityData, files.Logo, files.AdditionalImages);

        return Ok(new { 
            Message = "University created successfully"
        });
    }

    /// <summary>
    /// Get all universities with optional search query
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UniversityResponseDto>>> GetAll([FromQuery] string? query = null)
    {
        logger.LogInformation("Getting all universities with search query: {Query}", query ?? "none");
        var universities = await universityService.GetAllAsync(query);
        return Ok(universities);
    }

    /// <summary>
    /// Get university by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UniversityResponseDto>> GetById(Guid id)
    {
        try
        {
            var university = await universityService.GetByIdAsync(id);
            return Ok(university);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting university with ID {Id}", id);
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get university by institution ID
    /// </summary>
    [HttpGet("institution/{institutionId}")]
    public async Task<ActionResult<UniversityResponseDto>> GetByInstitutionId(Guid institutionId)
    {
        try
        {
            var university = await universityService.GetByInstitutionIdAsync(institutionId);
            return Ok(university);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting university for institution with ID {InstitutionId}", institutionId);
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Search universities by name, description, or other criteria
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<UniversityResponseDto>>> Search([FromQuery] string query)
    {
        logger.LogInformation("Searching universities with query: {Query}", query);
        var universities = await universityService.GetAllAsync(query);
        return Ok(universities);
    }

    /// <summary>
    /// Update an existing university
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UniversityResponseDto>> Update(Guid id, UpdateUniversityDto dto)
    {
        try
        {
            var university = await universityService.UpdateAsync(id, dto);
            return Ok(university);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating university with ID {Id}", id);
            return ex.Message.Contains("not found") 
                ? NotFound(new { message = ex.Message }) 
                : BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a university
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await universityService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting university with ID {Id}", id);
            return ex.Message.Contains("not found") 
                ? NotFound(new { message = ex.Message }) 
                : BadRequest(new { message = ex.Message });
        }
    }
}