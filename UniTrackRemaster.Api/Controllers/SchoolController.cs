using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Services.Organization;
using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Api.Dto.Request;

namespace UniTrackRemaster.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolController(ISchoolService schoolService) : ControllerBase
    {

        [HttpPost("init")]
        public async Task<IActionResult> InitSchool(
            [FromForm] InitSchoolDto schoolData,
            [FromForm] SchoolFilesModel files)
        {
            var schoolId = await schoolService.InitSchoolAsync(schoolData, files.Logo, files.AdditionalImages);

            return Ok(new { 
                Message = "School created successfully"
            });
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetSchools([FromQuery] SchoolFilterDto filter)
        {
            var schools = await schoolService.GetSchoolsAsync(filter);
            return Ok(schools);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSchool(Guid id)
        {
            var school = await schoolService.GetSchoolAsync(id);
            return Ok(school);
        }
    }
}
