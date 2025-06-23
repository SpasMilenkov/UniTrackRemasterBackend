using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Data.Repositories;

public class AcademicYearRepository : Repository<AcademicYear>, IAcademicYearRepository
{
    private readonly UniTrackDbContext _context;

    public AcademicYearRepository(UniTrackDbContext context) : base(context)
    {
        _context = context;
    }

    #region Basic CRUD operations

    public async Task<AcademicYear?> GetByIdAsync(Guid id)
    {
        return await _context.AcademicYears
            .Include(ay => ay.Semesters)
            .Include(ay => ay.Institution)
            .FirstOrDefaultAsync(ay => ay.Id == id);
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

    #endregion

    #region Basic count and exists methods

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AcademicYears.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AcademicYears.AnyAsync(ay => ay.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid institutionId, CancellationToken cancellationToken = default)
    {
        return await _context.AcademicYears.AnyAsync(ay => ay.Name == name && ay.InstitutionId == institutionId, cancellationToken);
    }

    #endregion

    #region Non-paginated methods (for calculations and internal operations)

    public async Task<IEnumerable<AcademicYear>> GetAllAsync(Guid institutionId)
    {
        return await _context.AcademicYears
            .Include(a => a.Semesters)
            .Where(a => a.InstitutionId == institutionId)
            .OrderByDescending(a => a.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AcademicYear>> GetByInstitutionAsync(Guid institutionId)
    {
        return await _context.AcademicYears
            .Include(ay => ay.Semesters)
            .Include(ay => ay.Institution)
            .Where(ay => ay.InstitutionId == institutionId)
            .OrderByDescending(ay => ay.StartDate)
            .ToListAsync();
    }

    public async Task<AcademicYear?> GetCurrentAsync(Guid institutionId)
    {
        var today = DateTime.UtcNow.Date;

        return await _context.AcademicYears
            .Include(a => a.Semesters)
            .Include(a => a.Institution)
            .Where(a => a.InstitutionId == institutionId)
            .Where(a => a.StartDate.Date <= today && a.EndDate.Date >= today)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<AcademicYear>> GetActiveAsync(Guid institutionId)
    {
        // Since AcademicYear doesn't have IsActive property, we'll consider "active" as current
        var today = DateTime.UtcNow.Date;
        return await _context.AcademicYears
            .Include(a => a.Semesters)
            .Include(a => a.Institution)
            .Where(a => a.InstitutionId == institutionId && 
                       a.StartDate <= today && a.EndDate >= today)
            .OrderByDescending(a => a.StartDate)
            .ToListAsync();
    }

    #endregion

    #region Paginated methods with filtering (for API endpoints)

    public async Task<List<AcademicYear>> GetByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        bool? isActive = null,
        bool? isCurrent = null,
        int page = 1,
        int pageSize = 50)
    {
        var queryable = _context.AcademicYears
            .Include(ay => ay.Semesters)
            .Include(ay => ay.Institution)
            .Where(ay => ay.InstitutionId == institutionId)
            .AsQueryable();

        queryable = ApplyFilters(queryable, query, isActive, isCurrent);

        return await queryable
            .OrderByDescending(ay => ay.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetByInstitutionCountAsync(
        Guid institutionId,
        string? query = null,
        bool? isActive = null,
        bool? isCurrent = null)
    {
        var queryable = _context.AcademicYears
            .Where(ay => ay.InstitutionId == institutionId)
            .AsQueryable();
        
        queryable = ApplyFilters(queryable, query, isActive, isCurrent);
        return await queryable.CountAsync();
    }

    // Helper method to apply filters
    private IQueryable<AcademicYear> ApplyFilters(
        IQueryable<AcademicYear> queryable,
        string? query,
        bool? isActive,
        bool? isCurrent)
    {
        // Text search across name only (no Description property on AcademicYear)
        if (!string.IsNullOrEmpty(query))
        {
            var searchTerm = query.ToLower();
            queryable = queryable.Where(ay => 
                ay.Name.ToLower().Contains(searchTerm));
        }

        // Active status filter - Note: AcademicYear doesn't have IsActive property
        // We'll consider an academic year "active" if it's current (within date range)
        if (isActive.HasValue && isActive.Value)
        {
            var today = DateTime.UtcNow.Date;
            queryable = queryable.Where(ay => ay.StartDate <= today && ay.EndDate >= today);
        }
        else if (isActive.HasValue && !isActive.Value)
        {
            var today = DateTime.UtcNow.Date;
            queryable = queryable.Where(ay => ay.StartDate > today || ay.EndDate < today);
        }

        // Current academic year filter (within date range)
        if (isCurrent.HasValue && isCurrent.Value)
        {
            var today = DateTime.UtcNow.Date;
            queryable = queryable.Where(ay => ay.StartDate <= today && ay.EndDate >= today);
        }

        return queryable;
    }

    #endregion

    #region Semester-related methods

    public async Task<bool> HasSemestersAsync(Guid academicYearId)
    {
        return await _context.Semesters
            .AnyAsync(s => s.AcademicYearId == academicYearId);
    }

    public async Task<int> GetSemesterCountAsync(Guid academicYearId)
    {
        return await _context.Semesters
            .CountAsync(s => s.AcademicYearId == academicYearId);
    }

    #endregion

    #region Validation methods

    public async Task<bool> IsNameUniqueAsync(string name, Guid institutionId, Guid? excludeId = null)
    {
        var query = _context.AcademicYears
            .Where(ay => ay.Name == name && ay.InstitutionId == institutionId);

        if (excludeId.HasValue)
        {
            query = query.Where(ay => ay.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<bool> HasDateOverlapAsync(Guid institutionId, DateTime startDate, DateTime endDate, Guid? excludeId = null)
    {
        var query = _context.AcademicYears
            .Where(ay => ay.InstitutionId == institutionId)
            .Where(ay => 
                (ay.StartDate <= endDate && ay.EndDate >= startDate)); // Date overlap check

        if (excludeId.HasValue)
        {
            query = query.Where(ay => ay.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    #endregion

    #region Statistics and reporting

    public async Task<AcademicYearStatistics> GetStatisticsAsync(Guid academicYearId)
    {
        var academicYear = await _context.AcademicYears
            .Include(ay => ay.Semesters)
            .FirstOrDefaultAsync(ay => ay.Id == academicYearId);

        if (academicYear == null)
            return new AcademicYearStatistics();

        var semesterCount = academicYear.Semesters.Count;
        var activeSemesterCount = academicYear.Semesters.Count(s => s.Status == SemesterStatus.Active);
        
        // You can extend this with more statistics as needed
        var studentCount = await _context.Students
            .Include(s => s.Grade)
            .Where(s => s.Grade != null && 
                       _context.AcademicYears
                           .Any(ay => ay.Id == academicYearId && 
                                     ay.InstitutionId == s.Grade.InstitutionId))
            .CountAsync();

        var today = DateTime.UtcNow.Date;
        return new AcademicYearStatistics
        {
            AcademicYearId = academicYearId,
            TotalSemesters = semesterCount,
            ActiveSemesters = activeSemesterCount,
            TotalStudents = studentCount,
            StartDate = academicYear.StartDate,
            EndDate = academicYear.EndDate,
            IsCurrent = today >= academicYear.StartDate && today <= academicYear.EndDate
        };
    }

    #endregion
}

// Helper class for statistics
