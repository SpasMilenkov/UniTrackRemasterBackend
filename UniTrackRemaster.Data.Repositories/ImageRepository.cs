using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Analytics;
using UniTrackRemaster.Data.Models.Images;

namespace UniTrackRemaster.Data.Repositories;


public class ImageRepository : IImageRepository
{
    private readonly UniTrackDbContext _context;

    public ImageRepository(UniTrackDbContext context)
    {
        _context = context;
    }

    public async Task<List<Image>> GetAllAsync()
    {
        return await _context.Images.ToListAsync();
    }

    public async Task<List<Image>> GetByInstitutionIdAsync(Guid institutionId)
    {
        return await _context.Images
            .Where(i => i.InstitutionId == institutionId)
            .ToListAsync();
    }

    public async Task<Image?> GetByIdAsync(Guid id)
    {
        return await _context.Images.FindAsync(id);
    }

    public async Task<Image> AddAsync(Image image)
    {
        await _context.Images.AddAsync(image);
        await _context.SaveChangesAsync();
        return image;
    }
    
    public async Task AddRangeAsync(IEnumerable<Image> images)
    {
        await _context.Images.AddRangeAsync(images);
    }

    public async Task AddBulkAsync(IEnumerable<Image> images)
    {
        await _context.Images.AddRangeAsync(images);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Image image)
    {
        _context.Images.Update(image);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var image = await GetByIdAsync(id);
        if (image is not null)
        {
            _context.Images.Remove(image);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Images.AnyAsync(i => i.Id == id);
    }
}
