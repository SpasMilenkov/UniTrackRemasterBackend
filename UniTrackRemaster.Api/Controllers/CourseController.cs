using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Course;
using UniTrackRemaster.Services.Academics;

namespace UniTrackRemaster.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CourseResponseDto>> GetById(Guid id)
    {
        return await _courseService.GetByIdAsync(id);
    }

    [HttpGet("semester/{semesterId}")]
    public async Task<ActionResult<IEnumerable<CourseResponseDto>>> GetBySemester(Guid semesterId)
    {
        return Ok(await _courseService.GetBySemesterAsync(semesterId));
    }
 
    [HttpGet("subject/{subjectId}")]
    public async Task<ActionResult<IEnumerable<CourseResponseDto>>> GetBySubject(Guid subjectId)
    {
        return Ok(await _courseService.GetBySubjectAsync(subjectId));
    }

    [HttpGet("major/{majorId}")]
    public async Task<ActionResult<IEnumerable<CourseResponseDto>>> GetByMajor(Guid majorId)
    {
        return Ok(await _courseService.GetByMajorAsync(majorId));
    }

    [HttpPost]
    public async Task<ActionResult<CourseResponseDto>> Create(CreateCourseDto dto)
    {
        var course = await _courseService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CourseResponseDto>> Update(Guid id, UpdateCourseDto dto)
    {
        return await _courseService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _courseService.DeleteAsync(id);
        return NoContent();
    }
}