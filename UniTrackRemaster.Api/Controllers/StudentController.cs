using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Services.Student;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;

namespace UniTrackRemaster.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StudentResponseDto>> GetById(Guid id)
        {
            return await _studentService.GetByIdAsync(id);
        }

        [HttpGet("school/{schoolId}")]
        public async Task<ActionResult<IEnumerable<StudentResponseDto>>> GetBySchool(Guid schoolId)
        {
            return Ok(await _studentService.GetBySchoolAsync(schoolId));
        }

        [HttpGet("university/{universityId}")]
        public async Task<ActionResult<IEnumerable<StudentResponseDto>>> GetByUniversity(Guid universityId)
        {
            return Ok(await _studentService.GetByUniversityAsync(universityId));
        }

        [HttpGet("grade/{gradeId}")]
        public async Task<ActionResult<IEnumerable<StudentResponseDto>>> GetByGrade(Guid gradeId)
        {
            return Ok(await _studentService.GetByGradeAsync(gradeId));
        }

        [HttpPost]
        public async Task<ActionResult<StudentResponseDto>> Create(CreateStudentDto dto)
        {
            var student = await _studentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<StudentResponseDto>> Update(Guid id, UpdateStudentDto dto)
        {
            return await _studentService.UpdateAsync(id, dto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _studentService.DeleteAsync(id);
            return NoContent();
        }
    }
}
