using Microsoft.AspNetCore.Http;
using UniTrackRemaster.Api.Dto.Institution;

namespace UniTrackRemaster.Services.Organization;

public interface IUniversityService
{
    Task<Guid> InitUniversityAsync(InitUniversityDto universityData, IFormFile? logo, List<IFormFile> images);
}