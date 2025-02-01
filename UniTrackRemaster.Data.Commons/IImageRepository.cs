using UniTrackRemaster.Data.Models.Images;

namespace UniTrackRemaster.Commons;

public interface IImageRepository
{
    Task<List<Image>> GetAllAsync();
    Task<List<Image>> GetByInstitutionIdAsync(Guid institutionId);
    Task<Image?> GetByIdAsync(Guid id);
    Task<Image> AddAsync(Image image);
    Task AddRangeAsync(IEnumerable<Image> images);
    Task AddBulkAsync(IEnumerable<Image> images);
    Task UpdateAsync(Image image);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
