using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Data.Repositories;

public class SemesterRepository : ISemesterRepository
{
    private readonly UniTrackDbContext _context;

    public SemesterRepository(UniTrackDbContext context)
    {
        _context = context;
    }

    public async Task<Semester?> GetByIdAsync(Guid id)
    {
        return await _context.Semesters
            .Include(s => s.AcademicYear)
            .Include(s => s.Courses)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Semester>> GetByAcademicYearAsync(Guid academicYearId)
    {
        return await _context.Semesters
            .Include(s => s.AcademicYear)
            .Include(s => s.Courses)
            .Where(s => s.AcademicYearId == academicYearId)
            .ToListAsync();
    }

    public async Task<Semester> CreateAsync(Semester semester)
    {
        _context.Semesters.Add(semester);
        await _context.SaveChangesAsync();
        return semester;
    }

    public async Task UpdateAsync(Semester semester)
    {
        _context.Semesters.Update(semester);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var semester = await GetByIdAsync(id);
        if (semester != null)
        {
            _context.Semesters.Remove(semester);
            await _context.SaveChangesAsync();
        }
    }
}