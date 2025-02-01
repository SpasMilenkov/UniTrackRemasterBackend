using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Repositories;

public class InstitutionRepository(UniTrackDbContext context) : IInstitutionRepository
{
    public async Task<Institution?> GetByIdAsync(Guid id)
    {
        return await context.Institutions
            .Include(e => e.Address)
            .Include(e => e.Images)
            .Include(e => e.Accreditations)
            .Include(e => e.Students)
            .Include(e => e.Teachers)
            .Include(e => e.Events)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
    public async Task<List<Institution>> GetInstitutionsByUserIdAsync(string userId)
    {
        // Get institutions through different user roles
        var institutions = await context.Institutions
            .Where(ei => 
                // Through Admin role
                ei.Admins.Any(a => a.UserId.ToString() == userId) ||
                // Through Teacher role
                ei.Teachers.Any(t => t.UserId.ToString() == userId) ||
                // Through Student role (both school and university)
                ei.Students.Any(s => s.UserId.ToString() == userId))
            .Include(ei => ei.Address)
            .Include(ei => ei.Images)
            .ToListAsync();

        return institutions;
    }
    public async Task<List<Institution>> GetAllAsync()
    {
        return await context.Institutions
            .Include(e => e.Address)
            .Include(e => e.Images)
            .ToListAsync();
    }

    public async Task<Institution> AddAsync(Institution entity)
    {
        await context.Institutions.AddAsync(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Institution entity)
    {
        context.Entry(entity).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await context.Institutions.FindAsync(id);
        if (entity != null)
        {
            context.Institutions.Remove(entity);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await context.Institutions.AnyAsync(e => e.Id == id);
    }
}