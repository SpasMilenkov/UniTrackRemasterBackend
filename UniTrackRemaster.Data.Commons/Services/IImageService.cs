using UniTrackRemaster.Api.Dto.Image;

namespace UniTrackRemaster.Commons.Services;

public interface IImageService
{
    Task<IEnumerable<ImageDto>> GetAllAsync();
    Task<IEnumerable<ImageDto>> GetByInstitutionIdAsync(Guid institutionId);
    Task<ImageDto?> GetByIdAsync(Guid id);
    Task<ImageDto> AddAsync(CreateImageDto imageDto);
    Task AddBulkAsync(List<string> urls, Guid institutionId);
    Task UpdateAsync(Guid id, string url);
    Task DeleteAsync(Guid id);
}
