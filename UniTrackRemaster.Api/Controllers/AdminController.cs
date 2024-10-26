using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UniTrackRemaster.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        [HttpGet("school/statistics")]
        public async Task<IActionResult> GetSchoolStatistics()
        {
            throw new NotImplementedException();
        }

        [HttpPost("student/approve")]
        public async Task<IActionResult> ApproveStudent()
        {
            throw new NotImplementedException();
        }

        [HttpPost("teacher/approve")]
        public async Task<IActionResult> ApproveTeacher()
        {
            throw new NotImplementedException();
        }

        [HttpPut("student/update")]
        public async Task<IActionResult> UpdateStudent()
        {
            throw new NotImplementedException();
        }

        [HttpPut("teacher/update")]
        public async Task<IActionResult> UpdateTeacher()
        {
            throw new NotImplementedException();
        }

        [HttpDelete("student/delete")]
        public async Task<IActionResult> DeleteStudent()
        {
            throw new NotImplementedException();
        }

        [HttpDelete("teacher/delete")]
        public async Task<IActionResult> DeleteTeacher()
        {
            throw new NotImplementedException();
        }
    }
}
