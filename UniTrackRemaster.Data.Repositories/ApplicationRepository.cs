using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Organizations;
using UniTrackRemaster.Services.Organization.Exceptions.Application;

namespace UniTrackRemaster.Data.Repositories;

public class ApplicationRepository : Repository<Application>, IApplicationRepository
{
    public ApplicationRepository(UniTrackDbContext context) : base(context) { }

    public async Task<Application?> GetByIdAsync(Guid id) => 
        await _context.Applications
            .Include(a => a.Institution)
            .ThenInclude(i => i!.Address)
            .FirstOrDefaultAsync(a => a.Id == id);
        
    public async Task<Application?> GetByInstitutionIdAsync(Guid id) => 
        await _context.Applications
            .Include(a => a.Institution)
            .ThenInclude(i => i!.Address)
            .FirstOrDefaultAsync(a => a.InstitutionId == id);

    public async Task<Application?> GetByEmailAsync(string email) => 
        await _context.Applications
            .Include(a => a.Institution)
            .ThenInclude(i => i!.Address)
            .FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower());

    public async Task<List<Application>> GetAllAsync(string? statusFilter = null, int page = 1, int pageSize = 50)
    {
        var query = _context.Applications
            .Include(a => a.Institution)
            .ThenInclude(i => i!.Address)
            .AsQueryable();

        // Apply status filter if provided
        if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<ApplicationStatus>(statusFilter, true, out var status))
        {
            query = query.Where(a => a.Status == status);
        }

        // Apply pagination
        return await query
            .OrderBy(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(string? statusFilter = null)
    {
        var query = _context.Applications.AsQueryable();

        if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<ApplicationStatus>(statusFilter, true, out var status))
        {
            query = query.Where(a => a.Status == status);
        }

        return await query.CountAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Applications
            .AnyAsync(a => a.Email.ToLower() == email.ToLower());
    }

    public async Task<Application> CreateAsync(Application application)
    {
        // Validate business rules
        if (await ExistsByEmailAsync(application.Email))
        {
            throw new DuplicateApplicationException(application.Email);
        }

        // Add to context (don't save - that's handled by UnitOfWork)
        await _context.Applications.AddAsync(application);
        return application;
    }

    public async Task<Application> UpdateAsync(Guid id, Application updatedApplication)
    {
        var application = await GetByIdAsync(id);
        
        if (application == null) 
        {
            throw new NotFoundException("Application", id.ToString());
        }

        // Update allowed properties
        application.FirstName = updatedApplication.FirstName;
        application.LastName = updatedApplication.LastName;
        application.Phone = updatedApplication.Phone;
        // Status should only be updated through specific methods
        
        _context.Applications.Update(application);
        return application;
    }

    public async Task<Application> ApproveAsync(Guid id)
    {
        var application = await GetByIdAsync(id);
        
        if (application == null)
        {
            throw new NotFoundException("Application", id.ToString());
        }
        if(application.Status is ApplicationStatus.Approved)
            throw new InvalidOperationException("Application is already approved");
        // Update status
        application.Status = ApplicationStatus.Approved;
        
        // Update institution status if needed
        if (application.Institution != null)
        {
            application.Institution.IntegrationStatus = IntegrationStatus.Verified;
        }

        _context.Applications.Update(application);
        return application;
    }

    public async Task<Application> RejectAsync(Guid id, string? reason = null)
    {
        var application = await GetByIdAsync(id);
        
        if (application == null)
        {
            throw new NotFoundException("Application", id.ToString());
        }

        // Validate that application can be rejected
        if (application.Status == ApplicationStatus.Denied)
        {
            throw new InvalidApplicationStateException(id, application.Status.ToString(), "Not already denied");
        }

        application.Status = ApplicationStatus.Denied;
        
        // Update institution status
        if (application.Institution != null)
        {
            application.Institution.IntegrationStatus = IntegrationStatus.Denied;
        }

        _context.Applications.Update(application);
        return application;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var application = await GetByIdAsync(id);
        
        if (application == null)
        {
            return false;
        }

        // Soft delete by setting status to denied
        await RejectAsync(id, "Application deleted");
        return true;
    }

    public async Task<bool> IsCodeValidAsync(string email, string code)
    {
        var application = await GetByEmailAsync(email);
        
        if (application == null)
        {
            return false;
        }

        // Check if code matches
        if (application.Code != code)
        {
            return false;
        }
        
        return true;
    }

    public async Task<Application> VerifyCodeAsync(string email, string code)
    {
        var application = await GetByEmailAsync(email);
        
        if (application == null)
        {
            throw new NotFoundException("Application", $"email: {email}");
        }

        // Check if code matches
        if (application.Code != code)
        {
            throw new InvalidVerificationCodeException(email, code);
        }
        
        _context.Applications.Update(application);
        return application;
    }
}

