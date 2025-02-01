using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Grade;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Services.Academics;

namespace UniTrackRemaster.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class GradesController : ControllerBase
    {
        private readonly IGradeService _gradeService;

        public GradesController(IGradeService gradeService)
        {
            _gradeService = gradeService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GradeResponseDto>> GetById(Guid id)
        {
            return await _gradeService.GetByIdAsync(id);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GradeResponseDto>>> GetAll()
        { 
            return Ok(await _gradeService.GetAllAsync());
        }

        [HttpPost]
        public async Task<ActionResult<GradeResponseDto>> Create(CreateGradeDto dto)
        {
            var grade = await _gradeService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = grade.Id }, grade);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<GradeResponseDto>> Update(Guid id, UpdateGradeDto dto)
        {
            return await _gradeService.UpdateAsync(id, dto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _gradeService.DeleteAsync(id);
            return NoContent();
        }
    }
}
