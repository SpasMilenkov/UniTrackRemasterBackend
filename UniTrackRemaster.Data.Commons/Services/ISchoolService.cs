using Microsoft.AspNetCore.Http;
using UniTrackRemaster.Api.Dto.Institution;

namespace UniTrackRemaster.Commons.Services;

public interface ISchoolService
{
    Task<Guid> InitSchoolAsync(InitSchoolDto schoolData, IFormFile? logo,List<IFormFile> images);
    Task<SchoolWithAddressResponseDto> GetSchoolAsync(Guid schoolId);
    Task<SchoolWithAddressResponseDto> GetSchoolByInstitutionIdAsync(Guid institutionId);
    Task<List<string>> UploadSchoolImagesAsync(Guid schoolId, List<IFormFile> images);
    Task<List<SchoolWithAddressResponseDto>> GetSchoolsAsync(SchoolFilterDto filter);
    Task<SchoolBaseResponseDto> UpdateSchoolAsync(UpdateSchoolDto updateDto);
    Task DeleteSchoolAsync(Guid schoolId);
}