using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Faculty;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Services.Academics;

namespace UniTrackRemaster.Controllers;


[ApiController]
[Route("api/universities/{universityId}/faculties")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class FacultiesController : ControllerBase
{
    private readonly IFacultyService _facultyService;

    public FacultiesController(IFacultyService facultyService)
    {
        _facultyService = facultyService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FacultyResponseDto>> GetById(Guid universityId, Guid id)
    {
        return await _facultyService.GetByIdAsync(id);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FacultyResponseDto>>> GetByUniversity(Guid universityId)
    {
        return Ok(await _facultyService.GetByUniversityAsync(universityId));
    }

    [HttpPost]
    public async Task<ActionResult<FacultyResponseDto>> Create(Guid universityId, CreateFacultyDto dto)
    {
        if (universityId != dto.UniversityId)
            return BadRequest("University ID mismatch");

        var faculty = await _facultyService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), 
            new { universityId, id = faculty.Id }, 
            faculty);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FacultyResponseDto>> Update(
        Guid universityId,
        Guid id,
        UpdateFacultyDto dto)
    {
        return await _facultyService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid universityId, Guid id)
    {
        await _facultyService.DeleteAsync(id);
        return NoContent();
    }
}
