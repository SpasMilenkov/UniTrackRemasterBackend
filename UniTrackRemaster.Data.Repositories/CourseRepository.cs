using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Data.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly UniTrackDbContext _context;

    public CourseRepository(UniTrackDbContext context)
    {
        _context = context;
    }

    public async Task<Course?> GetByIdAsync(Guid id)
    {
        return await _context.Courses
            .Include(c => c.Subject)
            .Include(c => c.Major)
            .Include(c => c.Semester)
            .Include(c => c.StudentCourses)
            .Include(c => c.Assignments)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Course>> GetBySemesterAsync(Guid semesterId)
    {
        return await _context.Courses
            .Include(c => c.Subject)
            .Include(c => c.Major)
            .Include(c => c.Semester)
            .Include(c => c.StudentCourses)
            .Include(c => c.Assignments)
            .Where(c => c.SemesterId == semesterId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetBySubjectAsync(Guid subjectId)
    {
        return await _context.Courses
            .Include(c => c.Subject)
            .Include(c => c.Major)
            .Include(c => c.Semester)
            .Include(c => c.StudentCourses)
            .Include(c => c.Assignments)
            .Where(c => c.SubjectId == subjectId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetByMajorAsync(Guid majorId)
    {
        return await _context.Courses
            .Include(c => c.Subject)
            .Include(c => c.Major)
            .Include(c => c.Semester)
            .Include(c => c.StudentCourses)
            .Include(c => c.Assignments)
            .Where(c => c.MajorId == majorId)
            .ToListAsync();
    }

    public async Task<Course> CreateAsync(Course course)
    {
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return course;
    }

    public async Task UpdateAsync(Course course)
    {
        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var course = await GetByIdAsync(id);
        if (course != null)
        {
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
        }
    }
}