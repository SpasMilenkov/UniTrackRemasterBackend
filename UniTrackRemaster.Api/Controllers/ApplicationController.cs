using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrganizationServices;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;

namespace UniTrackRemaster.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService _service;

        public ApplicationController(ApplicationService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationResponseDto>> GetApplicationById(Guid id)
        {
            var application = await _service.GetApplicationByIdAsync(id);
            if (application == null) return NotFound();
            return Ok(application);
        }

        [HttpGet]
        public async Task<ActionResult<List<ApplicationResponseDto>>> GetAllApplications() =>
            Ok(await _service.GetAllApplicationsAsync());

        [HttpPost]
        public async Task<ActionResult<ApplicationResponseDto>> CreateApplication(CreateSchoolApplicationDto dto)
        {
            var createdApplication = await _service.CreateApplicationAsync(dto);
            return CreatedAtAction(nameof(GetApplicationById), new { id = createdApplication.Id }, createdApplication);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateApplication(Guid id, CreateSchoolApplicationDto dto)
        {
            var updatedApplication = await _service.UpdateApplicationAsync(id, dto);
            if (updatedApplication == null) return NotFound();
            return Ok(updatedApplication);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApplication(Guid id)
        {
            var success = await _service.DeleteApplicationAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
