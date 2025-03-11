using UniTrackRemaster.Api.Dto.Image;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Exceptions;

namespace UniTrackRemaster.Services.Organization;
public class ImageService : IImageService
{
    private readonly IImageRepository _imageRepository;

    public ImageService(IImageRepository imageRepository)
    {
        _imageRepository = imageRepository;
    }

    public async Task<IEnumerable<ImageDto>> GetAllAsync()
    {
        var images = await _imageRepository.GetAllAsync();
        return images.Select(ImageDto.FromEntity);
    }

    public async Task<IEnumerable<ImageDto>> GetByInstitutionIdAsync(Guid institutionId)
    {
        var images = await _imageRepository.GetByInstitutionIdAsync(institutionId);
        return images.Select(ImageDto.FromEntity);
    }

    public async Task<ImageDto?> GetByIdAsync(Guid id)
    {
        var image = await _imageRepository.GetByIdAsync(id);
        return image != null ? ImageDto.FromEntity(image) : null;
    }

    public async Task<ImageDto> AddAsync(CreateImageDto imageDto)
    {
        var entity = CreateImageDto.ToEntity(imageDto);
        var created = await _imageRepository.AddAsync(entity);
        return ImageDto.FromEntity(created);
    }

    public async Task AddBulkAsync(List<string> urls, Guid institutionId)
    {
        var images = urls.Select(url => new CreateImageDto(institutionId, url))
            .Select(CreateImageDto.ToEntity)
            .ToList();
        
        await _imageRepository.AddBulkAsync(images);
    }

    public async Task UpdateAsync(Guid id, string url)
    {
        var image = await _imageRepository.GetByIdAsync(id);
        if (image is null)
            throw new NotFoundException($"Image with ID {id} not found.");

        image.Url = url;
        await _imageRepository.UpdateAsync(image);
    }

    public async Task DeleteAsync(Guid id)
    {
        if (!await _imageRepository.ExistsAsync(id))
            throw new NotFoundException($"Image with ID {id} not found.");

        await _imageRepository.DeleteAsync(id);
    }
}
