using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Services.Academics;

namespace UniTrackRemaster.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherService _teacherService;

        public TeachersController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TeacherResponseDto>> GetById(Guid id)
        {
            return await _teacherService.GetByIdAsync(id);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeacherResponseDto>>> GetAll()
        {
            return Ok(await _teacherService.GetAllAsync());
        }

        [HttpPost]
        public async Task<ActionResult<TeacherResponseDto>> Create(CreateTeacherDto dto)
        {
            var teacher = await _teacherService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = teacher.Id }, teacher);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TeacherResponseDto>> Update(Guid id, UpdateTeacherDto dto)
        {
            return await _teacherService.UpdateAsync(id, dto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _teacherService.DeleteAsync(id);
            return NoContent();
        }
    }
}
