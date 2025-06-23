using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Repositories;

public class AdminRepository(UniTrackDbContext context) : Repository<Admin>(context), IAdminRepository
{
    public async Task<IEnumerable<Admin>> GetAllAsync()
    {
        return await _context.Admins
            .Include(a => a.User)
            .Include(a => a.Institution)
            .ToListAsync();
    }

    public async Task<Admin?> GetByIdAsync(Guid id)
    {
        return await _context.Admins
            .Include(a => a.User)
            .Include(a => a.Institution)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Admin?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Admins
            .Include(a => a.User)
            .Include(a => a.Institution)
            .FirstOrDefaultAsync(a => a.UserId == userId);
    }

    public async Task<IEnumerable<Admin>> GetByInstitutionAsync(Guid institutionId)
    {
        return await _context.Admins
            .Include(a => a.User)
            .Include(a => a.Institution)
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
            .AnyAsync(a => a.InstitutionId == institutionId && a.Status == ProfileStatus.Active);
    }
    public async Task<IEnumerable<Admin>> GetPendingByUserIdAsync(Guid userId)
    {
        return await _context.Admins
            .Include(a => a.User)
            .Include(a => a.Institution)
            .Where(a => a.UserId == userId && a.Status == ProfileStatus.Pending)
            .ToListAsync();
    }

    public async Task<IEnumerable<Admin>> GetByInstitutionAsync(Guid institutionId, ProfileStatus? status = null)
    {
        var query = _context.Admins
            .Include(a => a.User)
            .Where(a => a.InstitutionId == institutionId);

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Admin>> GetByStatusAsync(ProfileStatus status)
    {
        return await _context.Admins
            .Include(a => a.User)
            .Include(a => a.Institution)
            .Where(a => a.Status == status)
            .ToListAsync();
    }
}