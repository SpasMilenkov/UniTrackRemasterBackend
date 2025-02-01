using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Services.Academics;

namespace UniTrackRemaster.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SubjectResponseDto>> GetById(Guid id)
    {
        return await _subjectService.GetByIdAsync(id);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SubjectResponseDto>>> GetAll()
    {
        return Ok(await _subjectService.GetAllAsync());
    }

    [HttpPost]
    public async Task<ActionResult<SubjectResponseDto>> Create(CreateSubjectDto dto)
    {
        var subject = await _subjectService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = subject.Id }, subject);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SubjectResponseDto>> Update(Guid id, UpdateSubjectDto dto)
    {
        return await _subjectService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _subjectService.DeleteAsync(id);
        return NoContent();
    }
}