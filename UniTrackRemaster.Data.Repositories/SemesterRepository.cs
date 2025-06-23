using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Data.Repositories;

public class SemesterRepository : Repository<Semester>, ISemesterRepository
{
    private readonly UniTrackDbContext _context;

    public SemesterRepository(UniTrackDbContext context) : base(context)
    {
        _context = context;
    }

    #region Basic CRUD operations

    public async Task<Semester?> GetByIdAsync(Guid id)
    {
        return await _context.Semesters
            .Include(s => s.AcademicYear)
                .ThenInclude(ay => ay.Institution)
            .FirstOrDefaultAsync(s => s.Id == id);
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

    #endregion

    #region Basic count and exists methods

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Semesters.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Semesters.AnyAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid academicYearId, CancellationToken cancellationToken = default)
    {
        return await _context.Semesters.AnyAsync(s => s.Name == name && s.AcademicYearId == academicYearId, cancellationToken);
    }

    #endregion

    #region Non-paginated methods (for calculations and internal operations)

    public async Task<IEnumerable<Semester>> GetAllAsync()
    {
        return await _context.Semesters
            .Include(s => s.AcademicYear)
                .ThenInclude(ay => ay.Institution)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Semester>> GetByAcademicYearAsync(Guid academicYearId)
    {
        return await _context.Semesters
            .Include(s => s.AcademicYear)
                .ThenInclude(ay => ay.Institution)
            .Where(s => s.AcademicYearId == academicYearId)
            .OrderBy(s => s.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Semester>> GetActiveAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Semesters
            .Include(s => s.AcademicYear)
                .ThenInclude(ay => ay.Institution)
            .Where(s => s.StartDate <= today && s.EndDate >= today && s.Status == SemesterStatus.Active)
            .OrderBy(s => s.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Semester>> GetActiveAsync(Guid academicYearId)
    {
        return await _context.Semesters
            .Include(s => s.AcademicYear)
                .ThenInclude(ay => ay.Institution)
            .Where(s => s.AcademicYearId == academicYearId && s.Status == SemesterStatus.Active)
            .OrderBy(s => s.StartDate)
            .ToListAsync();
    }

    public async Task<Semester?> GetCurrentAsync(Guid academicYearId)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Semesters
            .Include(s => s.AcademicYear)
                .ThenInclude(ay => ay.Institution)
            .Where(s => s.AcademicYearId == academicYearId)
            .Where(s => s.StartDate <= today && s.EndDate >= today)
            .FirstOrDefaultAsync();
    }

    public async Task<Semester?> GetCurrentActiveAsync(Guid institutionId)
    {
        var now = DateTime.UtcNow;
        return await _context.Semesters
            .Include(s => s.AcademicYear)
            .Where(s => s.AcademicYear.InstitutionId == institutionId &&
                        s.Status == SemesterStatus.Active &&
                        s.StartDate <= now &&
                        s.EndDate >= now)
            .FirstOrDefaultAsync();
    }


    public async Task<IEnumerable<Semester>> GetByInstitutionAsync(Guid institutionId)
    {
        return await _context.Semesters
            .Include(s => s.AcademicYear)
                .ThenInclude(ay => ay.Institution)
            .Where(s => s.AcademicYear.InstitutionId == institutionId)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Semester>> GetCurrentByInstitutionAsync(Guid institutionId)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Semesters
            .Include(s => s.AcademicYear)
                .ThenInclude(ay => ay.Institution)
            .Where(s => s.AcademicYear.InstitutionId == institutionId)
            .Where(s => s.StartDate <= today && s.EndDate >= today)
            .OrderBy(s => s.StartDate)
            .ToListAsync();
    }

    #endregion

    #region Paginated methods with filtering (for API endpoints)

    public async Task<List<Semester>> GetByAcademicYearAsync(
        Guid academicYearId,
        string? query = null,
        string? status = null,
        string? type = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 50)
    {
        var queryable = _context.Semesters
            .Include(s => s.AcademicYear)
                .ThenInclude(ay => ay.Institution)
            .Where(s => s.AcademicYearId == academicYearId)
            .AsQueryable();

        queryable = ApplyFilters(queryable, query, status, type, isActive);

        return await queryable
            .OrderBy(s => s.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetByAcademicYearCountAsync(
        Guid academicYearId,
        string? query = null,
        string? status = null,
        string? type = null,
        bool? isActive = null)
    {
        var queryable = _context.Semesters
            .Where(s => s.AcademicYearId == academicYearId)
            .AsQueryable();
        
        queryable = ApplyFilters(queryable, query, status, type, isActive);
        return await queryable.CountAsync();
    }

    // Helper method to apply filters
    private IQueryable<Semester> ApplyFilters(
        IQueryable<Semester> queryable,
        string? query,
        string? status,
        string? type,
        bool? isActive)
    {
        // Text search across name and description
        if (!string.IsNullOrEmpty(query))
        {
            var searchTerm = query.ToLower();
            queryable = queryable.Where(s => 
                (s.Name != null && s.Name.ToLower().Contains(searchTerm)) ||
                (s.Description != null && s.Description.ToLower().Contains(searchTerm)));
        }

        // Status filter
        if (!string.IsNullOrEmpty(status) &&
            Enum.TryParse<SemesterStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            queryable = queryable.Where(s => s.Status == parsedStatus);
        }

        // Type filter
        if (!string.IsNullOrEmpty(type) &&
            Enum.TryParse<SemesterType>(type, ignoreCase: true, out var parsedType))
        {
            queryable = queryable.Where(s => s.Type == parsedType);
        }

        // Active status filter (calculated based on current date and status)
        if (isActive.HasValue)
        {
            var today = DateTime.UtcNow.Date;
            if (isActive.Value)
            {
                queryable = queryable.Where(s => 
                    s.Status == SemesterStatus.Active &&
                    s.StartDate <= today && 
                    s.EndDate >= today);
            }
            else
            {
                queryable = queryable.Where(s => 
                    s.Status != SemesterStatus.Active ||
                    s.StartDate > today || 
                    s.EndDate < today);
            }
        }

        return queryable;
    }

    #endregion

    #region Date and status related methods

    public async Task<Semester?> GetByDateAsync(DateTime date, Guid institutionId)
    {
        return await _context.Semesters
            .Include(s => s.AcademicYear)
                .ThenInclude(ay => ay.Institution)
            .Where(s => s.AcademicYear.InstitutionId == institutionId)
            .Where(s => s.StartDate <= date && s.EndDate >= date)
            .FirstOrDefaultAsync();
    }

    public async Task<Semester?> GetPreviousSemesterAsync(Guid semesterId)
    {
        var currentSemester = await GetByIdAsync(semesterId);
        if (currentSemester == null) return null;

        // First try to find a previous semester in the same academic year
        var previousSemester = await _context.Semesters
            .Include(s => s.AcademicYear)
                .ThenInclude(ay => ay.Institution)
            .Where(s => s.AcademicYearId == currentSemester.AcademicYearId)
            .Where(s => s.EndDate < currentSemester.StartDate)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync();

        if (previousSemester != null) return previousSemester;

        // If no previous semester in the same academic year, look for the last semester of the previous academic year
        var academicYear = await _context.AcademicYears
            .FirstOrDefaultAsync(a => a.Id == currentSemester.AcademicYearId);

        if (academicYear == null) return null;

        var previousAcademicYear = await _context.AcademicYears
            .Where(a => a.InstitutionId == academicYear.InstitutionId)
            .Where(a => a.EndDate < academicYear.StartDate)
            .OrderByDescending(a => a.EndDate)
            .FirstOrDefaultAsync();

        if (previousAcademicYear == null) return null;

        // Get the last semester of the previous academic year
        return await _context.Semesters
            .Include(s => s.AcademicYear)
                .ThenInclude(ay => ay.Institution)
            .Where(s => s.AcademicYearId == previousAcademicYear.Id)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync();
    }

    public async Task<Semester?> GetNextSemesterAsync(Guid semesterId)
    {
        var currentSemester = await GetByIdAsync(semesterId);
        if (currentSemester == null) return null;

        // First try to find a next semester in the same academic year
        var nextSemester = await _context.Semesters
            .Include(s => s.AcademicYear)
                .ThenInclude(ay => ay.Institution)
            .Where(s => s.AcademicYearId == currentSemester.AcademicYearId)
            .Where(s => s.StartDate > currentSemester.EndDate)
            .OrderBy(s => s.StartDate)
            .FirstOrDefaultAsync();

        if (nextSemester != null) return nextSemester;

        // If no next semester in the same academic year, look for the first semester of the next academic year
        var academicYear = await _context.AcademicYears
            .FirstOrDefaultAsync(a => a.Id == currentSemester.AcademicYearId);

        if (academicYear == null) return null;

        var nextAcademicYear = await _context.AcademicYears
            .Where(a => a.InstitutionId == academicYear.InstitutionId)
            .Where(a => a.StartDate > academicYear.EndDate)
            .OrderBy(a => a.StartDate)
            .FirstOrDefaultAsync();

        if (nextAcademicYear == null) return null;

        // Get the first semester of the next academic year
        return await _context.Semesters
            .Include(s => s.AcademicYear)
                .ThenInclude(ay => ay.Institution)
            .Where(s => s.AcademicYearId == nextAcademicYear.Id)
            .OrderBy(s => s.StartDate)
            .FirstOrDefaultAsync();
    }

    #endregion

    #region Validation methods

    public async Task<bool> IsNameUniqueAsync(string name, Guid academicYearId, Guid? excludeId = null)
    {
        var query = _context.Semesters
            .Where(s => s.Name == name && s.AcademicYearId == academicYearId);

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<bool> HasDateOverlapAsync(Guid academicYearId, DateTime startDate, DateTime endDate, Guid? excludeId = null)
    {
        var query = _context.Semesters
            .Where(s => s.AcademicYearId == academicYearId)
            .Where(s => 
                (s.StartDate <= endDate && s.EndDate >= startDate)); // Date overlap check

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
    // TODO: Implement This
    public async Task<bool> HasCoursesAsync(Guid semesterId)
    {
        // Check if semester has associated courses/subjects
        // Note: Adjust this based on your actual data model structure
        // This assumes you have subjects/courses that reference semesters
        
        // If you have a direct Course -> Semester relationship:
        // return await _context.Courses.AnyAsync(c => c.SemesterId == semesterId);
        
        // If you have subjects that are offered in semesters through another relationship:
        // For now, return false until you clarify the relationship structure
        return false;
        
        // Alternative: Check if there are any enrollments, grades, or other related data
        // return await _context.Enrollments.AnyAsync(e => e.Semester.Id == semesterId);
    }

    #endregion

    #region Statistics and reporting

    public async Task<SemesterStatistics> GetStatisticsAsync(Guid semesterId)
    {
        var semester = await _context.Semesters
            .Include(s => s.AcademicYear)
            .FirstOrDefaultAsync(s => s.Id == semesterId);

        if (semester == null)
            return new SemesterStatistics();

        // Calculate statistics - adjust based on your actual data model
        var courseCount = 0; // await _context.Courses.CountAsync(c => c.SemesterId == semesterId);
        var studentCount = 0; // await _context.Enrollments.Where(e => e.Course.SemesterId == semesterId).Select(e => e.StudentId).Distinct().CountAsync();

        // You can implement these based on your actual entity relationships:
        // - If you have Course entity with SemesterId
        // - If you have Enrollment/StudentCourse entities
        // - If you have Grade entities with semester relationships

        return new SemesterStatistics
        {
            SemesterId = semesterId,
            Name = semester.Name,
            Type = semester.Type,
            Status = semester.Status,
            StartDate = semester.StartDate,
            EndDate = semester.EndDate,
            WeekCount = semester.WeekCount,
            TotalCourses = courseCount,
            TotalStudents = studentCount,
            IsCurrent = DateTime.UtcNow.Date >= semester.StartDate && 
                       DateTime.UtcNow.Date <= semester.EndDate,
            DaysRemaining = semester.EndDate > DateTime.UtcNow.Date ? 
                           (semester.EndDate - DateTime.UtcNow.Date).Days : 0
        };
    }

    #endregion

    #region Bulk operations

    public async Task<IEnumerable<Semester>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _context.Semesters
            .Include(s => s.AcademicYear)
                .ThenInclude(ay => ay.Institution)
            .Where(s => ids.Contains(s.Id))
            .ToListAsync();
    }

    public async Task UpdateStatusBulkAsync(IEnumerable<Guid> semesterIds, SemesterStatus newStatus)
    {
        var semesters = await _context.Semesters
            .Where(s => semesterIds.Contains(s.Id))
            .ToListAsync();

        foreach (var semester in semesters)
        {
            semester.Status = newStatus;
        }

        await _context.SaveChangesAsync();
    }

    #endregion
}

