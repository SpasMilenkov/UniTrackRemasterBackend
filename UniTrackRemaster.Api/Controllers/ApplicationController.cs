using Microsoft.AspNetCore.Mvc;
using OrganizationServices;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;

namespace UniTrackRemaster.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationController(IApplicationService service) : ControllerBase
    {

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApplicationResponseDto>> GetApplicationById(Guid id)
        {
            var application = await service.GetApplicationByIdAsync(id);
            if (application == null) return NotFound();
            return Ok(application);
        }

        [HttpGet("school/{schoolId:guid}")]
        public async Task<ActionResult<ApplicationResponseDto>> GetApplicationBySchoolId(Guid schoolId)
        {
            var application = await service.GetApplicationBySchoolIdAsync(schoolId);
            if(application == null) return NotFound();
            return Ok(application);
        }

        [HttpPut("approve")]
        public async Task<IActionResult> ApproveApplication(Guid applicationId)
        {
            await service.ApproveApplicationAsync(applicationId);
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<List<ApplicationResponseDto>>> GetAllApplications() =>
            Ok(await service.GetAllApplicationsAsync());

        [HttpPost]
        public async Task<ActionResult<ApplicationResponseDto>> CreateApplication(CreateSchoolApplicationDto dto)
        {
            var createdApplication = await service.CreateApplicationAsync(dto);
            return CreatedAtAction(nameof(GetApplicationById), new { id = createdApplication.Id }, createdApplication);
        }

        [HttpPost("login-with-code")]
        public async Task<ActionResult<ApplicationResponseDto>> LoginWithCode(string code, string email)
        {
            var application = await service.GetApplicationByCodeAsync(code, email);
            return Ok(application);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateApplication(Guid id, UpdateSchoolApplicationDto dto)
        {
            var updatedApplication = await service.UpdateApplicationAsync(id, dto);
            if (updatedApplication == null) return NotFound();
            return Ok(updatedApplication);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteApplication(Guid id)
        {
            var success = await service.DeleteApplicationAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
