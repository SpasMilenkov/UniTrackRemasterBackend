using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Data.Repositories;

public class MajorRepository(UniTrackDbContext context) : Repository<Major>(context), IMajorRepository
{
    public async Task<Major?> GetByIdAsync(Guid id)
    {
        return await _context.Majors
            .Include(m => m.Faculty)
            .Include(m => m.Students)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Major>> GetByFacultyAsync(Guid facultyId)
    {
        return await _context.Majors
            .Include(m => m.Faculty)
            .Include(m => m.Students)
            .Where(m => m.FacultyId == facultyId)
            .ToListAsync();
    }

    public async Task<Major> CreateAsync(Major major)
    {
        _context.Majors.Add(major);
        await _context.SaveChangesAsync();
        return major;
    }

    public async Task UpdateAsync(Major major)
    {
        _context.Majors.Update(major);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var major = await GetByIdAsync(id);
        if (major != null)
        {
            _context.Majors.Remove(major);
            await _context.SaveChangesAsync();
        }
    }
}
