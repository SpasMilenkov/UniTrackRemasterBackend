using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UniTrackRemaster.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        [HttpGet("school/statistics")]
        public Task<IActionResult> GetSchoolStatistics()
        {
            throw new NotImplementedException();
        }

        [HttpPost("student/approve")]
        public Task<IActionResult> ApproveStudent()
        {
            throw new NotImplementedException();
        }

        [HttpPost("teacher/approve")]
        public async Task<IActionResult> ApproveTeacher()
        {
            throw new NotImplementedException();
        }

        [HttpPut("student/update")]
        public Task<IActionResult> UpdateStudent()
        {
            throw new NotImplementedException();
        }

        [HttpPut("teacher/update")]
        public Task<IActionResult> UpdateTeacher()
        {
            throw new NotImplementedException();
        }

        [HttpDelete("student/delete")]
        public Task<IActionResult> DeleteStudent()
        {
            throw new NotImplementedException();
        }

        [HttpDelete("teacher/delete")]
        public Task<IActionResult> DeleteTeacher()
        {
            throw new NotImplementedException();
        }
    }
}
