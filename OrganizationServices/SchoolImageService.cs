using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Models.Images;

namespace OrganizationServices;


public class SchoolImageService(ISchoolImageRepository schoolImageRepository) : ISchoolImageService
{

    public async Task<IEnumerable<SchoolImage>> GetAllAsync()
    {
        return await schoolImageRepository.GetAllAsync();
    }

    public async Task<SchoolImage?> GetByIdAsync(string id)
    {
        return await schoolImageRepository.GetByIdAsync(id);
    }

    public async Task AddAsync(CreateSchoolImageDto schoolImageDto)
    {
        await schoolImageRepository.AddAsync(CreateSchoolImageDto.ToEntity(schoolImageDto));
    }

    public async Task AddBulkAsync(List<string> urls, Guid schoolId)
    {
        var schoolImages = urls.Select(u => new CreateSchoolImageDto { Url = u, SchoolId = schoolId });
        
        await schoolImageRepository.AddBulkAsync(schoolImages.Select(CreateSchoolImageDto.ToEntity));
    }

    public async Task UpdateAsync(Guid schoolId, string imagePath)
    {
        await schoolImageRepository.UpdateAsync(schoolId, imagePath);
    }

    public async Task DeleteAsync(string id)
    {
        await schoolImageRepository.DeleteAsync(id);
    }
}