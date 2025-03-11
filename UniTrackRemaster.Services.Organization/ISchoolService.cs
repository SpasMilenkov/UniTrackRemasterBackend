using Microsoft.AspNetCore.Http;
using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;

namespace UniTrackRemaster.Services.Organization;

public interface ISchoolService
{
    Task<Guid> InitSchoolAsync(InitSchoolDto schoolData, IFormFile? logo,List<IFormFile> images);
    Task<SchoolWithAddressResponseDto> GetSchoolAsync(Guid schoolId);
    Task<List<string>> UploadSchoolImagesAsync(Guid schoolId, List<IFormFile> images);
    Task<List<SchoolWithAddressResponseDto>> GetSchoolsAsync(SchoolFilterDto filter);
    Task<SchoolBaseResponseDto> UpdateSchoolAsync(UpdateSchoolDto updateDto);
    Task DeleteSchoolAsync(Guid schoolId);
}