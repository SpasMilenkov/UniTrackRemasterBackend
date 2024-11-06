using Microsoft.AspNetCore.Mvc;
using OrganizationServices;
using StorageService;
using UniTrackRemaster.Api.Dto.Request;

namespace UniTrackRemaster.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolController(ISchoolService schoolService, IFirebaseStorageService storageService, ISchoolImageService schoolImageService) : ControllerBase
    {
        [HttpPost("init")]
        public async Task<IActionResult> InitSchool(
            [FromForm] InitSchoolDto schoolData,
            [FromForm] List<IFormFile> files)
        {
            var schoolId = await schoolService.InitSchoolAsync(schoolData);
            // Log or process the schoolData if needed
            Console.WriteLine($"School Name: {schoolData.Name}");

            var fileUrls = new List<string>();

            // Upload each file to Firebase and store the URLs
            foreach (var file in files)
            {
                if (file.Length <= 0) continue;
                var fileUrl = await storageService.UploadFileAsync(file);
                fileUrls.Add(fileUrl); // Collect each file's URL for response or processing
            }

            await schoolImageService.AddBulkAsync(fileUrls, schoolId);
            
            // Return success response, including uploaded file URLs if needed
            return Ok(new { Message = "School created successfully", FileUrls = fileUrls });
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetSchools(int page = 1, int pageSize = 10)
        {
            var schools = await schoolService.GetSchoolsAsync(page, pageSize);
            return Ok(schools);
        }
    }
}
