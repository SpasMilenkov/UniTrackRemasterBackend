using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Repositories;

public class ApplicationRepository(UniTrackDbContext context): IApplicationRepository
{
    public async Task<Application?> GetApplicationByIdAsync(Guid id) => await context.Applications
            .Include(a => a.School)
            .FirstOrDefaultAsync(a => a.Id == id);
        
    

    public async Task<List<Application>> GetAllApplicationsAsync() => await context.Applications.Include(a => a.School).ToListAsync();

    public async Task<Application> CreateApplicationAsync(Application application)
    {
        context.Applications.Add(application);
        await context.SaveChangesAsync();
        return application;
    }

    public async Task<Application?> UpdateApplicationAsync(Guid id, Application updatedApplication)
    {
        var application = await context.Applications.FindAsync(id);
        if (application == null) return null;

        application.FirstName = updatedApplication.FirstName;
        application.LastName = updatedApplication.LastName;
        application.Email = updatedApplication.Email;
        application.Phone = updatedApplication.Phone;

        await context.SaveChangesAsync();
        return application;
    }

    public async Task<bool> DeleteApplicationAsync(Guid id)
    {
        var application = await context.Applications.FindAsync(id);
        if (application == null) return false;
        
        context.Applications.Remove(application);
        await context.SaveChangesAsync();
        return true;
    }
}