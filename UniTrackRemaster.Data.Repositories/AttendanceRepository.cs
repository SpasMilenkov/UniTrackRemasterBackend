using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Data.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly UniTrackDbContext _context;

    public AttendanceRepository(UniTrackDbContext context)
    {
        _context = context;
    }

    public async Task<Attendance?> GetByIdAsync(Guid id)
    {
        return await _context.Attendances
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Course)
            .Include(a => a.Subject)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Attendance>> GetByStudentAsync(Guid studentId)
    {
        return await _context.Attendances
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Course)
            .Include(a => a.Subject)
            .Where(a => a.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Attendance>> GetByCourseAsync(Guid courseId)
    {
        return await _context.Attendances
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Course)
            .Where(a => a.CourseId == courseId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Attendance>> GetBySubjectAsync(Guid subjectId)
    {
        return await _context.Attendances
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Subject)
            .Where(a => a.SubjectId == subjectId)
            .ToListAsync();
    }

    public async Task<Attendance> CreateAsync(Attendance attendance)
    {
        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();
        return attendance;
    }

    public async Task UpdateAsync(Attendance attendance)
    {
        _context.Attendances.Update(attendance);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var attendance = await GetByIdAsync(id);
        if (attendance != null)
        {
            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();
        }
    }
}