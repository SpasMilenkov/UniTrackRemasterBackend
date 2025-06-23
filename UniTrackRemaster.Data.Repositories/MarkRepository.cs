using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Api.Dto.Mark;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Data.Repositories;

/// <summary>
/// Mark repository implementation with semester support
/// </summary>
public class MarkRepository : Repository<Mark>, IMarkRepository
{
    private readonly UniTrackDbContext _context;

    public MarkRepository(UniTrackDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Mark?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Marks
            .Include(m => m.Subject)
            .Include(m => m.Teacher)
            .ThenInclude(t => t.User)
            .Include(m => m.Student)
            .ThenInclude(s => s.User)
            .Include(m => m.Semester)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Mark>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Marks
            .Include(m => m.Subject)
            .Include(m => m.Teacher)
            .ThenInclude(t => t.User)
            .Include(m => m.Student)
            .ThenInclude(s => s.User)
            .Include(m => m.Semester)
            .ToListAsync(cancellationToken);
    }

    public async Task<Mark> AddAsync(Mark mark, CancellationToken cancellationToken = default)
    {
        await _context.Marks.AddAsync(mark, cancellationToken);
        return mark;
    }

    public async Task UpdateAsync(Mark mark, CancellationToken cancellationToken = default)
    {
        _context.Marks.Update(mark);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mark = await _context.Marks.FindAsync(new object[] { id }, cancellationToken);
        if (mark != null)
        {
            _context.Marks.Remove(mark);
        }
    }

    public async Task<IEnumerable<Mark>> GetMarksByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Marks
            .Where(m => m.StudentId == studentId)
            .Include(m => m.Subject)
            .Include(m => m.Teacher)
            .ThenInclude(t => t.User)
            .Include(m => m.Student)
            .ThenInclude(s => s.User)
            .Include(m => m.Semester)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Mark>> GetMarksByStudentIdAsync(Guid studentId, Guid? semesterId, CancellationToken cancellationToken = default)
    {
        IQueryable<Mark> query = _context.Marks
            .Where(m => m.StudentId == studentId)
            .Include(m => m.Subject)
            .Include(m => m.Teacher)
            .ThenInclude(t => t.User)
            .Include(m => m.Student)
            .ThenInclude(s => s.User)
            .Include(m => m.Semester);

        if (semesterId.HasValue)
            query = query.Where(m => m.SemesterId == semesterId.Value);

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Mark>> GetMarksByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = default)
    {
        return await _context.Marks
            .Where(m => m.TeacherId == teacherId)
            .Include(m => m.Subject)
            .Include(m => m.Teacher)
            .ThenInclude(t => t.User)
            .Include(m => m.Student)
            .ThenInclude(s => s.User)
            .Include(m => m.Semester)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Mark>> GetMarksByTeacherIdAsync(Guid teacherId, Guid? semesterId, CancellationToken cancellationToken = default)
    {
        IQueryable<Mark> query = _context.Marks
            .Where(m => m.TeacherId == teacherId)
            .Include(m => m.Subject)
            .Include(m => m.Teacher)
            .ThenInclude(t => t.User)
            .Include(m => m.Student)
            .ThenInclude(s => s.User)
            .Include(m => m.Semester);

        if (semesterId.HasValue)
            query = query.Where(m => m.SemesterId == semesterId.Value);

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }


    public async Task<IEnumerable<Mark>> GetMarksBySubjectIdAsync(Guid subjectId, CancellationToken cancellationToken = default)
    {
        return await _context.Marks
            .Where(m => m.SubjectId == subjectId)
            .Include(m => m.Subject)
            .Include(m => m.Teacher)
            .ThenInclude(t => t.User)
            .Include(m => m.Student)
            .ThenInclude(s => s.User)
            .Include(m => m.Semester)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }


    public async Task<IEnumerable<Mark>> GetMarksBySubjectIdAsync(Guid subjectId, Guid? semesterId, CancellationToken cancellationToken = default)
    {
        IQueryable<Mark> query = _context.Marks
            .Where(m => m.SubjectId == subjectId)
            .Include(m => m.Subject)
            .Include(m => m.Teacher)
            .ThenInclude(t => t.User)
            .Include(m => m.Student)
            .ThenInclude(s => s.User)
            .Include(m => m.Semester);

        if (semesterId.HasValue)
            query = query.Where(m => m.SemesterId == semesterId.Value);

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Mark>> GetMarksByFilterAsync(MarkFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        IQueryable<Mark> query = _context.Marks
            .Include(m => m.Subject)
            .Include(m => m.Teacher)
            .ThenInclude(t => t.User)
            .Include(m => m.Student)
            .ThenInclude(s => s.User)
            .Include(m => m.Semester);

        if (filterParams.StudentId.HasValue)
            query = query.Where(m => m.StudentId == filterParams.StudentId.Value);

        if (filterParams.TeacherId.HasValue)
            query = query.Where(m => m.TeacherId == filterParams.TeacherId.Value);

        if (filterParams.SubjectId.HasValue)
            query = query.Where(m => m.SubjectId == filterParams.SubjectId.Value);

        if (filterParams.SemesterId.HasValue)
            query = query.Where(m => m.SemesterId == filterParams.SemesterId.Value);

        if (filterParams.Type.HasValue)
            query = query.Where(m => m.Type == filterParams.Type.Value);

        if (filterParams.FromDate.HasValue)
            query = query.Where(m => m.CreatedAt >= filterParams.FromDate.Value);

        if (filterParams.ToDate.HasValue)
            query = query.Where(m => m.CreatedAt <= filterParams.ToDate.Value);

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetAverageMarkForStudentAsync(Guid studentId, Guid? subjectId = null, CancellationToken cancellationToken = default)
    {
        IQueryable<Mark> query = _context.Marks.Where(m => m.StudentId == studentId);

        if (subjectId.HasValue)
            query = query.Where(m => m.SubjectId == subjectId.Value);

        var marks = await query.ToListAsync(cancellationToken);
        return marks.Any() ? marks.Average(m => m.Value) : 0;
    }

    public async Task<decimal> GetAverageMarkForStudentAsync(Guid studentId, Guid? subjectId = null, Guid? semesterId = null, CancellationToken cancellationToken = default)
    {
        IQueryable<Mark> query = _context.Marks.Where(m => m.StudentId == studentId);

        if (subjectId.HasValue)
            query = query.Where(m => m.SubjectId == subjectId.Value);

        if (semesterId.HasValue)
            query = query.Where(m => m.SemesterId == semesterId.Value);

        var marks = await query.ToListAsync(cancellationToken);
        return marks.Any() ? marks.Average(m => m.Value) : 0;
    }

    public async Task<decimal> GetAverageMarkForSubjectAsync(Guid subjectId, CancellationToken cancellationToken = default)
    {
        var marks = await _context.Marks
            .Where(m => m.SubjectId == subjectId)
            .ToListAsync(cancellationToken);
        
        return marks.Any() ? marks.Average(m => m.Value) : 0;
    }

    public async Task<decimal> GetAverageMarkForSubjectAsync(Guid subjectId, Guid? semesterId = null, CancellationToken cancellationToken = default)
    {
        IQueryable<Mark> query = _context.Marks.Where(m => m.SubjectId == subjectId);

        if (semesterId.HasValue)
            query = query.Where(m => m.SemesterId == semesterId.Value);

        var marks = await query.ToListAsync(cancellationToken);
        return marks.Any() ? marks.Average(m => m.Value) : 0;
    }
}