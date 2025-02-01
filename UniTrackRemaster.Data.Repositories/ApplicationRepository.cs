using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly UniTrackDbContext _context;

    public ApplicationRepository(UniTrackDbContext context)
    {
        _context = context;
    }

    public async Task<Application?> GetByIdAsync(Guid id) => 
        await _context.Applications
            .Include(a => a.Institution)
            .ThenInclude(i => i.Address)
            .FirstOrDefaultAsync(a => a.Id == id);
        
    public async Task<Application?> GetByInstitutionIdAsync(Guid id) => 
        await _context.Applications
            .Include(a => a.Institution)
            .ThenInclude(i => i.Address)
            .FirstOrDefaultAsync(a => a.InstitutionId == id);

    public async Task<Application?> GetByEmailAsync(string email) => 
        await _context.Applications
            .Include(a => a.Institution)
            .ThenInclude(i => i.Address)
            .FirstOrDefaultAsync(a => a.Email == email);

    public async Task<List<Application>> GetAllAsync() => 
        await _context.Applications
            .Include(a => a.Institution)
            .ThenInclude(i => i.Address)
            .ToListAsync();

    public async Task<Application> CreateAsync(Application application)
    {
        await _context.Applications.AddAsync(application);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(application.Id);
    }

    public async Task<Application?> UpdateAsync(Guid id, Application updatedApplication)
    {
        var application = await _context.Applications
            .Include(a => a.Institution)
            .ThenInclude(i => i.Address)
            .FirstOrDefaultAsync(a => a.Id == id);
            
        if (application == null) 
            return null;

        application.FirstName = updatedApplication.FirstName;
        application.LastName = updatedApplication.LastName;
        application.Phone = updatedApplication.Phone;
        application.Status = updatedApplication.Status;

        await _context.SaveChangesAsync();
        return application;
    }

    public async Task<Application?> ApproveAsync(Guid id)
    {
        var application = await _context.Applications
            .Include(a => a.Institution)
            .ThenInclude(i => i.Address)
            .FirstOrDefaultAsync(a => a.Id == id);
            
        if (application is null) 
            return null;
        
        application.Status = ApplicationStatus.Approved;
        application.Institution.IntegrationStatus = IntegrationStatus.Verified;

        await _context.SaveChangesAsync();
        return application;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var application = await _context.Applications
            .Include(a => a.Institution)
            .FirstOrDefaultAsync(a => a.Id == id);
            
        if (application == null) 
            return false;
        
        application.Status = ApplicationStatus.Denied;
        application.Institution.IntegrationStatus = IntegrationStatus.Denied;
        
        await _context.SaveChangesAsync();
        return true;
    }
}
