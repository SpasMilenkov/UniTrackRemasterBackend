using Microsoft.AspNetCore.Http;
using UniTrackRemaster.Api.Dto.Institution;

namespace OrganizationServices;

public interface IUniversityService
{
    Task<Guid> InitUniversityAsync(InitUniversityDto universityData, IFormFile? logo, List<IFormFile> images);
}