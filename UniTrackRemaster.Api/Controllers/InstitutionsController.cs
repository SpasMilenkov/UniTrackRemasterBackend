using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Services.Organization;
using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Data.Exceptions;

namespace UniTrackRemaster.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstitutionsController(IInstitutionService institutionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InstitutionDto>>> GetAll()
    {
        var institutions = await institutionService.GetAllAsync();
        return Ok(institutions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InstitutionDto>> GetById(Guid id)
    {
        try
        {
            var institution = await institutionService.GetByIdAsync(id);
            return Ok(institution);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateInstitutionDto updateDto)
    {
        try
        {
            await institutionService.UpdateAsync(id, updateDto);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<InstitutionDto>>> GetUserInstitutions(string userId)
    {
        // Verify the requesting user can only access their own institutions
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId != userId)
        {
            return Forbid();
        }

        var institutions = await institutionService.GetInstitutionsByUserId(userId);
        return Ok(institutions);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await institutionService.DeleteAsync(id);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}