using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Api.Dto.Teacher.Analytics;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Data.Repositories;


/// <summary>
/// Absence repository implementation with semester support
/// </summary>
public class AbsenceRepository : Repository<Absence>, IAbsenceRepository
{
    private readonly UniTrackDbContext _context;

    public AbsenceRepository(UniTrackDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Absence?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Absences
            .Include(a => a.Student)
            .ThenInclude(s => s.User)
            .Include(a => a.Teacher)
            .ThenInclude(t => t.User)
            .Include(a => a.Subject)
            .Include(a => a.Semester)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Absence>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Absences
            .Include(a => a.Student)
            .ThenInclude(s => s.User)
            .Include(a => a.Teacher)
            .ThenInclude(t => t.User)
            .Include(a => a.Subject)
            .Include(a => a.Semester)
            .OrderByDescending(a => a.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<Absence> AddAsync(Absence absence, CancellationToken cancellationToken = default)
    {
        await _context.Absences.AddAsync(absence, cancellationToken);
        return absence;
    }

    public async Task UpdateAsync(Absence absence, CancellationToken cancellationToken = default)
    {
        _context.Absences.Update(absence);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var absence = await _context.Absences.FindAsync(new object[] { id }, cancellationToken);
        if (absence != null)
        {
            _context.Absences.Remove(absence);
        }
    }

    public async Task<IEnumerable<Absence>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Absences
            .Where(a => a.StudentId == studentId)
            .Include(a => a.Student)
            .ThenInclude(s => s.User)
            .Include(a => a.Teacher)
            .ThenInclude(t => t.User)
            .Include(a => a.Subject)
            .Include(a => a.Semester)
            .OrderByDescending(a => a.Date)
            .ToListAsync(cancellationToken);
    }

public async Task<IEnumerable<Absence>> GetByStudentIdAsync(Guid studentId, Guid? semesterId, CancellationToken cancellationToken = default)
{
    IQueryable<Absence> query = _context.Absences
        .Where(a => a.StudentId == studentId)
        .Include(a => a.Student)
            .ThenInclude(s => s.User)
        .Include(a => a.Teacher)
            .ThenInclude(t => t.User)
        .Include(a => a.Subject)
        .Include(a => a.Semester);

    if (semesterId.HasValue)
        query = query.Where(a => a.SemesterId == semesterId.Value);

    return await query
        .OrderByDescending(a => a.Date)
        .ToListAsync(cancellationToken);
}

public async Task<IEnumerable<Absence>> GetAbsencesByTeacherAsync(Guid teacherId, Guid? semesterId = null)
{
    IQueryable<Absence> query = _context.Absences
        .Where(a => a.TeacherId == teacherId)
        .Include(a => a.Student)
            .ThenInclude(s => s.User)
        .Include(a => a.Teacher)
            .ThenInclude(t => t.User)
        .Include(a => a.Subject)
        .Include(a => a.Semester);

    if (semesterId.HasValue)
        query = query.Where(a => a.SemesterId == semesterId.Value);

    return await query
        .OrderByDescending(a => a.Date)
        .ToListAsync();
}

public async Task<IEnumerable<Absence>> GetByStudentAndSubjectAsync(Guid studentId, Guid subjectId, Guid? semesterId = null)
{
    IQueryable<Absence> query = _context.Absences
        .Where(a => a.StudentId == studentId && a.SubjectId == subjectId)
        .Include(a => a.Student)
            .ThenInclude(s => s.User)
        .Include(a => a.Teacher)
            .ThenInclude(t => t.User)
        .Include(a => a.Subject)
        .Include(a => a.Semester);

    if (semesterId.HasValue)
        query = query.Where(a => a.SemesterId == semesterId.Value);

    return await query
        .OrderByDescending(a => a.Date)
        .ToListAsync();
}

public async Task<IEnumerable<Absence>> GetBySubjectAsync(Guid subjectId, Guid? semesterId = null)
{
    IQueryable<Absence> query = _context.Absences
        .Where(a => a.SubjectId == subjectId)
        .Include(a => a.Student)
            .ThenInclude(s => s.User)
        .Include(a => a.Teacher)
            .ThenInclude(t => t.User)
        .Include(a => a.Subject)
        .Include(a => a.Semester);

    if (semesterId.HasValue)
        query = query.Where(a => a.SemesterId == semesterId.Value);

    return await query
        .OrderByDescending(a => a.Date)
        .ToListAsync();
}


    public async Task<IEnumerable<Absence>> GetTeacherAttendanceDataAsync(
        Guid teacherId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        Guid? gradeId = null,
        Guid? subjectId = null,
        Guid? semesterId = null)
    {
        // Convert DateTime parameters to UTC to avoid PostgreSQL timezone issues
        var fromDateUtc = fromDate?.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc)
            : fromDate?.ToUniversalTime();

        var toDateUtc = toDate?.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc)
            : toDate?.ToUniversalTime();

        // Start with a basic query for absences where the teacher is directly assigned
        var query = _context.Absences.AsNoTracking()
            .Where(a => a.TeacherId == teacherId);

        // Apply semester filter if specified
        if (semesterId.HasValue) 
            query = query.Where(a => a.SemesterId == semesterId.Value);

        // Apply subject filter if specified
        if (subjectId.HasValue) 
            query = query.Where(a => a.SubjectId == subjectId.Value);

        // Apply grade filter if specified
        if (gradeId.HasValue) 
            query = query.Where(a => a.Student.GradeId == gradeId.Value);

        // Apply date range filters if specified
        if (fromDateUtc.HasValue) 
            query = query.Where(a => a.Date >= fromDateUtc.Value);

        if (toDateUtc.HasValue) 
            query = query.Where(a => a.Date <= toDateUtc.Value);

        // Include related entities and return
        return await query
            .Include(a => a.Student)
            .ThenInclude(s => s.User)
            .Include(a => a.Subject)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }
}
