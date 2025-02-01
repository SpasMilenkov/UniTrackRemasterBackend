using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Data.Repositories;

public class FacultyRepository : IFacultyRepository
{
    private readonly UniTrackDbContext _context;

    public FacultyRepository(UniTrackDbContext context)
    {
        _context = context;
    }

    public async Task<Faculty?> GetByIdAsync(Guid id)
    {
        return await _context.Faculties
            .Include(f => f.University)
            .Include(f => f.Majors)
            .Include(f => f.Departments)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<Faculty>> GetByUniversityAsync(Guid universityId)
    {
        return await _context.Faculties
            .Include(f => f.University)
            .Include(f => f.Majors)
            .Include(f => f.Departments)
            .Where(f => f.UniversityId == universityId)
            .ToListAsync();
    }

    public async Task<Faculty> CreateAsync(Faculty faculty)
    {
        _context.Faculties.Add(faculty);
        await _context.SaveChangesAsync();
        return faculty;
    }

    public async Task UpdateAsync(Faculty faculty)
    {
        _context.Faculties.Update(faculty);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var faculty = await GetByIdAsync(id);
        if (faculty != null)
        {
            _context.Faculties.Remove(faculty);
            await _context.SaveChangesAsync();
        }
    }
}
