using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Data.Repositories;

public class GradeRepository : IGradeRepository
{
    private readonly UniTrackDbContext _context;

    public GradeRepository(UniTrackDbContext context)
    {
        _context = context;
    }

    public async Task<Grade?> GetByIdAsync(Guid id)
    {
        return await _context.Grades
            .Include(g => g.Students)
            // .Include(g => g.ElectiveSubjects)
            .Include(g => g.HomeRoomTeacher)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<Grade>> GetAllAsync()
    {
        return await _context.Grades
            .Include(g => g.Students)
            // .Include(g => g.ElectiveSubjects)
            .Include(g => g.HomeRoomTeacher)
            .ToListAsync();
    }

    public async Task<Grade> CreateAsync(Grade grade)
    {
        _context.Grades.Add(grade);
        await _context.SaveChangesAsync();
        return grade;
    }

    public async Task UpdateAsync(Grade grade)
    {
        _context.Grades.Update(grade);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var grade = await GetByIdAsync(id);
        if (grade != null)
        {
            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();
        }
    }
}
