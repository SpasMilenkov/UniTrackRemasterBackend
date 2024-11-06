using UniTrackRemaster.Data.Models.Analytics;
using UniTrackRemaster.Data.Models.Images;

namespace UniTrackRemaster.Commons;

public interface ISchoolImageRepository
{
    Task<List<SchoolImage>> GetAllAsync();
    Task<SchoolImage?> GetByIdAsync(string id);
    Task AddAsync(SchoolImage schoolImage);
    Task AddBulkAsync(IEnumerable<SchoolImage> schoolImages);
    Task DeleteAsync(string id);
    Task UpdateAsync(Guid schoolImage, string imagePath);
}