using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.AcademicYear;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Services.Academics;

namespace UniTrackRemaster.Controllers;

[ApiController]
[Route("api/institutions/{institutionId}/academic-years")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AcademicYearsController : ControllerBase
{
    private readonly IAcademicYearService _academicYearService;

    public AcademicYearsController(IAcademicYearService academicYearService)
    {
        _academicYearService = academicYearService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AcademicYearResponseDto>> GetById(Guid institutionId, Guid id)
    {
        return await _academicYearService.GetByIdAsync(id);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AcademicYearResponseDto>>> GetByInstitution(Guid institutionId)
    {
        return Ok(await _academicYearService.GetByInstitutionAsync(institutionId));
    }

    [HttpPost]
    public async Task<ActionResult<AcademicYearResponseDto>> Create(Guid institutionId, CreateAcademicYearDto dto)
    {
        if (institutionId != dto.InstitutionId)
            return BadRequest("Institution ID mismatch");
            
        var academicYear = await _academicYearService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), 
            new { institutionId, id = academicYear.Id }, 
            academicYear);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AcademicYearResponseDto>> Update(
        Guid institutionId, 
        Guid id, 
        UpdateAcademicYearDto dto)
    {
        return await _academicYearService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid institutionId, Guid id)
    {
        await _academicYearService.DeleteAsync(id);
        return NoContent();
    }
}