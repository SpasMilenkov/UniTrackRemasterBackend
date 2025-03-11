using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Services.Organization;
using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Api.Dto.Request;

namespace UniTrackRemaster.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UniversitiesController : ControllerBase
{
    private readonly IUniversityService universityService;

    public UniversitiesController(IUniversityService universityService)
    {
        this.universityService = universityService;
    }

    [HttpPost("init")]
    public async Task<IActionResult> InitUniversity(
        [FromForm] InitUniversityDto universityData,
        [FromForm] UniversityFilesModel files)
    {
        var universityId = await universityService.InitUniversityAsync(universityData, files.Logo, files.AdditionalImages);

        return Ok(new { 
            Message = "University created successfully"
        });
    }
}