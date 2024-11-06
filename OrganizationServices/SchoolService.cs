using StorageService;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Models.Organizations;

namespace OrganizationServices;

public class SchoolService(ISchoolRepository schoolRepository, IFirebaseStorageService firebaseStorage) : ISchoolService
{

    public async Task<Guid> CreateSchoolAsync(string name, AddressRequestDto address)
    {
        var school = await schoolRepository.CreateSchoolAsync(name, AddressRequestDto.ToEntity(address));
        return school.Id;
    }

    public async Task<Guid> InitSchoolAsync(InitSchoolDto initDto)
    {
        var school = await schoolRepository.InitSchoolAsync(initDto);
        return school.Id;
    }

    public async Task<SchoolResponseDto> GetSchoolAsync(Guid schoolId)
    {
        var school = await schoolRepository.GetSchoolAsync(schoolId);

        // Create signed URLs for the school's images asynchronously
        var signedUrls = await Task.WhenAll(school.Images
            .Select(i => firebaseStorage.CreateSignedUrl(i.Url)));

        // Create the SchoolResponseDto with signed URLs for images
        var schoolDto = SchoolResponseDto.FromEntity(school);
        schoolDto.Images = signedUrls; // Set the signed URLs

        return schoolDto;
    }


    public async Task<List<SchoolResponseDto>> GetSchoolsAsync(int page = 0, int pageSize = 5)
    {
         var schools = await schoolRepository.GetSchoolsAsync(page, pageSize);

        var schoolDtos = new List<SchoolResponseDto>();

        foreach (var school in schools)
        {
            // Create signed URLs for images asynchronously
            if (school.Images != null)
            {
                var signedUrls = await Task.WhenAll(school.Images
                    .Select(i => firebaseStorage.CreateSignedUrl(i.Url)));

                // Create the SchoolResponseDto with the signed URLs
                var schoolDto = SchoolResponseDto.FromEntity(school);
                schoolDto.Images = signedUrls; // Set the signed URLs

                schoolDtos.Add(schoolDto);
            }
        }

         return schoolDtos;
    }


    public async Task<SchoolResponseDto> UpdateSchoolAsync(UpdateSchoolDto updateDto)
    {
        var school =  await schoolRepository.UpdateSchoolAsync(updateDto);
        
        return SchoolResponseDto.FromEntity(school);
    }

    public async Task DeleteSchoolAsync(Guid schoolId)
    {
        await schoolRepository.DeleteSchoolAsync(schoolId);
    }
}