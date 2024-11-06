using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Events;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Repositories;

public class ApplicationRepository(UniTrackDbContext context): IApplicationRepository
{
    public async Task<Application?> GetApplicationByIdAsync(Guid id) => await context.Applications
            .Include(a => a.School)
            .ThenInclude(s => s.Address)
            .FirstOrDefaultAsync(a => a.Id == id);
        
    public async Task<Application?> GetApplicationBySchoolIdAsync(Guid id) => await context.Applications
        .Include(a => a.School)
        .ThenInclude(s => s.Address)
        .FirstOrDefaultAsync(a => a.SchoolId == id);

    public async Task<Application?> GetApplicationByEmailAsync(string email) => await context.Applications
        .Include(a => a.School)
        .ThenInclude(s => s.Address)
        .FirstOrDefaultAsync(a => a.Email == email); 

    public async Task<List<Application>> GetAllApplicationsAsync() => await context.Applications
        .Include(a => a.School)
        .ThenInclude(s => s.Address)
        .ToListAsync();

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

    public async Task<Application> ApproveApplication(Guid id)
    {
        var application = await context.Applications
            .Include(a => a.School)
            .ThenInclude(s => s.Address)
            .FirstOrDefaultAsync(a => a.Id == id);
        if(application is null) return null;
        
        application.School.IntegrationStatus = IntegrationStatus.Verified;

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