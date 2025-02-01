using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Data.Repositories;

public class AcademicYearRepository : IAcademicYearRepository
{
    private readonly UniTrackDbContext _context;

    public AcademicYearRepository(UniTrackDbContext context)
    {
        _context = context;
    }

    public async Task<AcademicYear?> GetByIdAsync(Guid id)
    {
        return await _context.AcademicYears
            .Include(ay => ay.Semesters)
            .FirstOrDefaultAsync(ay => ay.Id == id);
    }

    public async Task<IEnumerable<AcademicYear>> GetByInstitutionAsync(Guid institutionId)
    {
        return await _context.AcademicYears
            .Include(ay => ay.Semesters)
            .Where(ay => ay.InstitutionId == institutionId)
            .ToListAsync();
    }

    public async Task<AcademicYear> CreateAsync(AcademicYear academicYear)
    {
        _context.AcademicYears.Add(academicYear);
        await _context.SaveChangesAsync();
        return academicYear;
    }

    public async Task UpdateAsync(AcademicYear academicYear)
    {
        _context.AcademicYears.Update(academicYear);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var academicYear = await GetByIdAsync(id);
        if (academicYear != null)
        {
            _context.AcademicYears.Remove(academicYear);
            await _context.SaveChangesAsync();
        }
    }
}