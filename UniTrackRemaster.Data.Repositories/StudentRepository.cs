using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly UniTrackDbContext _context;

    public StudentRepository(UniTrackDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(Guid id)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.School)
            .Include(s => s.University)
            .Include(s => s.Grade)
            .Include(s => s.Marks)
            .Include(s => s.AttendanceRecords)
            .Include(s => s.ClubMemberships)
            .Include(s => s.Electives)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Student?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<IEnumerable<Student>> GetBySchoolAsync(Guid schoolId)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Grade)
            .Where(s => s.SchoolId == schoolId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetByUniversityAsync(Guid universityId)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Grade)
            .Where(s => s.UniversityId == universityId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetByGradeAsync(Guid gradeId)
    {
        return await _context.Students
            .Include(s => s.User)
            .Where(s => s.GradeId == gradeId)
            .ToListAsync();
    }

    public async Task<Student> CreateAsync(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task UpdateAsync(Student student)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var student = await GetByIdAsync(id);
        if (student != null)
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }
    }
}