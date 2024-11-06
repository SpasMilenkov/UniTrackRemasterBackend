using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Analytics;
using UniTrackRemaster.Data.Models.Images;

namespace UniTrackRemaster.Data.Repositories;


public class SchoolImageRepository(UniTrackDbContext context) : ISchoolImageRepository
{

    public async Task<List<SchoolImage>> GetAllAsync()
    {
        return await context.SchoolImages.ToListAsync();
    }

    public async Task<SchoolImage?> GetByIdAsync(string id)
    {
        return await context.SchoolImages.FindAsync(id);
    }

    public async Task AddAsync(SchoolImage request)
    {
        var schoolImage = await context.SchoolImages.FindAsync(request.SchoolId);
        if(schoolImage is not null) throw new InvalidOperationException();

        var entity = new SchoolImage
        {
            SchoolId = request.SchoolId,
            Url = request.Url,
        };
        await context.SchoolImages.AddAsync(entity);
        await context.SaveChangesAsync();
    }

    public async Task AddBulkAsync(IEnumerable<SchoolImage> schoolImages)
    {
        foreach (var image in schoolImages)
            context.SchoolImages.Add(image);
        
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var schoolImage = await GetByIdAsync(id);
        if (schoolImage is not  null)
        {
            context.SchoolImages.Remove(schoolImage);
            await context.SaveChangesAsync();
        }
    }

    public async Task UpdateAsync(Guid schoolId, string url)
    {
        var schoolImage = await context.SchoolImages.FirstOrDefaultAsync(s => s.SchoolId == schoolId);
        if (schoolImage is null) throw new InvalidOperationException();
        schoolImage.Url = url;
        context.SchoolImages.Update(schoolImage);
        await context.SaveChangesAsync();
    }
}