using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Major;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Services.Academics;

namespace UniTrackRemaster.Controllers;

[ApiController]
[Route("api/faculties/{facultyId}/majors")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class MajorsController : ControllerBase
{
    private readonly IMajorService _majorService;

    public MajorsController(IMajorService majorService)
    {
        _majorService = majorService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MajorResponseDto>> GetById(Guid facultyId, Guid id)
    {
        return await _majorService.GetByIdAsync(id);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MajorResponseDto>>> GetByFaculty(Guid facultyId)
    {
        return Ok(await _majorService.GetByFacultyAsync(facultyId));
    }

    [HttpPost]
    public async Task<ActionResult<MajorResponseDto>> Create(Guid facultyId, CreateMajorDto dto)
    {
        if (facultyId != dto.FacultyId)
            return BadRequest("Faculty ID mismatch");

        var major = await _majorService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), 
            new { facultyId, id = major.Id }, 
            major);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MajorResponseDto>> Update(
        Guid facultyId,
        Guid id,
        UpdateMajorDto dto)
    {
        return await _majorService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid facultyId, Guid id)
    {
        await _majorService.DeleteAsync(id);
        return NoContent();
    }
}