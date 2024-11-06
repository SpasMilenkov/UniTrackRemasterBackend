using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Data.Models.Images;

namespace OrganizationServices;

public interface ISchoolImageService
{
    Task<IEnumerable<SchoolImage>> GetAllAsync();
    Task<SchoolImage?> GetByIdAsync(string id);
    Task AddAsync(CreateSchoolImageDto schoolImageDto);
    Task AddBulkAsync(List<string> urls, Guid schoolId);
    Task UpdateAsync(Guid schoolId, string imagePath);
    Task DeleteAsync(string id);
}