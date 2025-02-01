using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Department;
using UniTrackRemaster.Services.Academics;

namespace UniTrackRemaster.Controllers;

[ApiController]
[Route("api/faculties/{facultyId}/departments")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DepartmentResponseDto>> GetById(Guid facultyId, Guid id)
    {
        return await _departmentService.GetByIdAsync(id);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentResponseDto>>> GetByFaculty(Guid facultyId)
    {
        return Ok(await _departmentService.GetByFacultyAsync(facultyId));
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentResponseDto>> Create(Guid facultyId, CreateDepartmentDto dto)
    {
        if (facultyId != dto.FacultyId)
            return BadRequest("Faculty ID mismatch");

        var department = await _departmentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), 
            new { facultyId, id = department.Id }, 
            department);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DepartmentResponseDto>> Update(
        Guid facultyId,
        Guid id,
        UpdateDepartmentDto dto)
    {
        return await _departmentService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid facultyId, Guid id)
    {
        await _departmentService.DeleteAsync(id);
        return NoContent();
    }
}