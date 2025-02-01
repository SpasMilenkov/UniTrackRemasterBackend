using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly UniTrackDbContext _context;

    public AdminRepository(UniTrackDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Admin>> GetAllAsync()
    {
        return await _context.Admins
            .Include(a => a.User)
            .Include(a => a.Institution)
            .Include(a => a.Permissions)
            .ToListAsync();
    }

    public async Task<Admin?> GetByIdAsync(Guid id)
    {
        return await _context.Admins
            .Include(a => a.User)
            .Include(a => a.Institution)
            .Include(a => a.Permissions)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Admin?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Admins
            .Include(a => a.User)
            .Include(a => a.Institution)
            .Include(a => a.Permissions)
            .FirstOrDefaultAsync(a => a.UserId == userId);
    }

    public async Task<IEnumerable<Admin>> GetByInstitutionAsync(Guid institutionId)
    {
        return await _context.Admins
            .Include(a => a.User)
            .Include(a => a.Institution)
            .Include(a => a.Permissions)
            .Where(a => a.InstitutionId == institutionId)
            .ToListAsync();
    }

    public async Task<Admin> CreateAsync(Admin admin)
    {
        await _context.Admins.AddAsync(admin);
        await _context.SaveChangesAsync();
        return admin;
    }

    public async Task UpdateAsync(Admin admin)
    {
        _context.Entry(admin).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var admin = await _context.Admins.FindAsync(id);
        if (admin != null)
        {
            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Admins.AnyAsync(a => a.Id == id);
    }

    public async Task<bool> HasActiveAdminAsync(Guid institutionId)
    {
        return await _context.Admins
            .AnyAsync(a => a.InstitutionId == institutionId && a.Status == AdminStatus.Active);
    }
}