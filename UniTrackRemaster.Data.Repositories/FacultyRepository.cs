using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Data.Repositories;

public class FacultyRepository : Repository<Faculty>, IFacultyRepository
{
    public FacultyRepository(UniTrackDbContext context) : base(context) { }

    #region Basic CRUD operations

    public async Task<Faculty?> GetByIdAsync(Guid id)
    {
        return await _context.Faculties
            .Include(f => f.University)
            .Include(f => f.Departments)
            .Include(f => f.Majors)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Faculty?> GetByIdWithRelationsAsync(Guid id)
    {
        return await _context.Faculties
            .Include(f => f.University)
                .ThenInclude(u => u.Institution)
            .Include(f => f.Departments)
            .Include(f => f.Majors)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Faculty> CreateAsync(Faculty faculty)
    {
        _context.Faculties.Add(faculty);
        await _context.SaveChangesAsync();
        return faculty;
    }

    public async Task UpdateAsync(Faculty faculty)
    {
        _context.Faculties.Update(faculty);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var faculty = await GetByIdAsync(id);
        if (faculty != null)
        {
            _context.Faculties.Remove(faculty);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Basic count and exists methods

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Faculties.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Faculties.AnyAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid universityId, CancellationToken cancellationToken = default)
    {
        return await _context.Faculties.AnyAsync(f => f.Name == name && f.UniversityId == universityId, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, Guid universityId, CancellationToken cancellationToken = default)
    {
        return await _context.Faculties.AnyAsync(f => f.Code == code && f.UniversityId == universityId, cancellationToken);
    }

    public async Task<bool> HasDepartmentsAsync(Guid facultyId, CancellationToken cancellationToken = default)
    {
        return await _context.Faculties
            .AnyAsync(f => f.Id == facultyId && f.Departments.Any(), cancellationToken);
    }

    public async Task<bool> HasMajorsAsync(Guid facultyId, CancellationToken cancellationToken = default)
    {
        return await _context.Faculties
            .AnyAsync(f => f.Id == facultyId && f.Majors.Any(), cancellationToken);
    }

    #endregion

    #region Non-paginated methods (for calculations and internal operations)

    public async Task<IEnumerable<Faculty>> GetAllAsync()
    {
        return await _context.Faculties
            .Include(f => f.University)
            .Include(f => f.Departments)
            .Include(f => f.Majors)
            .ToListAsync();
    }

    public async Task<IEnumerable<Faculty>> GetAllWithRelationsAsync()
    {
        return await _context.Faculties
            .Include(f => f.University)
                .ThenInclude(u => u.Institution)
            .Include(f => f.Departments)
            .Include(f => f.Majors)
            .ToListAsync();
    }

    public async Task<IEnumerable<Faculty>> GetByUniversityAsync(Guid universityId)
    {
        return await _context.Faculties
            .Include(f => f.University)
            .Include(f => f.Departments)
            .Include(f => f.Majors)
            .Where(f => f.UniversityId == universityId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Faculty>> GetByUniversityWithRelationsAsync(Guid universityId)
    {
        return await _context.Faculties
            .Include(f => f.University)
                .ThenInclude(u => u.Institution)
            .Include(f => f.Departments)
            .Include(f => f.Majors)
            .Where(f => f.UniversityId == universityId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Faculty>> GetByInstitutionAsync(Guid institutionId)
    {
        return await _context.Faculties
            .Include(f => f.University)
            .Include(f => f.Departments)
            .Include(f => f.Majors)
            .Where(f => f.University.InstitutionId == institutionId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Faculty>> GetByInstitutionWithRelationsAsync(Guid institutionId)
    {
        return await _context.Faculties
            .Include(f => f.University)
                .ThenInclude(u => u.Institution)
            .Include(f => f.Departments)
            .Include(f => f.Majors)
            .Where(f => f.University.InstitutionId == institutionId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Faculty>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        searchTerm = searchTerm.ToLower();

        return await _context.Faculties
            .Include(f => f.University)
            .Include(f => f.Departments)
            .Include(f => f.Majors)
            .Where(f => f.Name.ToLower().Contains(searchTerm) ||
                   (f.Code != null && f.Code.ToLower().Contains(searchTerm)) ||
                   (f.ShortDescription != null && f.ShortDescription.ToLower().Contains(searchTerm)) ||
                   (f.DetailedDescription != null && f.DetailedDescription.ToLower().Contains(searchTerm)))
            .ToListAsync();
    }

    public async Task<IEnumerable<Faculty>> SearchWithRelationsAsync(string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
            return await GetAllWithRelationsAsync();

        return await _context.Faculties
            .Include(f => f.University)
                .ThenInclude(u => u.Institution)
            .Include(f => f.Departments)
            .Include(f => f.Majors)
            .Where(f => f.Name.Contains(searchTerm) ||
                   (f.Code != null && f.Code.Contains(searchTerm)) ||
                   (f.ShortDescription != null && f.ShortDescription.Contains(searchTerm)) ||
                   (f.DetailedDescription != null && f.DetailedDescription.Contains(searchTerm)))
            .ToListAsync();
    }

    #endregion

    #region Paginated methods with filtering (for API endpoints)

    public async Task<List<Faculty>> GetAllWithRelationsAsync(
        string? query = null,
        string? universityId = null,
        string? institutionId = null,
        string? status = null,
        int page = 1, 
        int pageSize = 50)
    {
        var queryable = _context.Faculties
            .Include(f => f.University)
                .ThenInclude(u => u.Institution)
            .Include(f => f.Departments)
            .Include(f => f.Majors)
            .AsQueryable();

        queryable = ApplyFilters(queryable, query, universityId, institutionId, status);

        return await queryable
            .OrderBy(f => f.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(
        string? query = null,
        string? universityId = null,
        string? institutionId = null,
        string? status = null)
    {
        var queryable = _context.Faculties.AsQueryable();
        queryable = ApplyFilters(queryable, query, universityId, institutionId, status);
        return await queryable.CountAsync();
    }

    public async Task<List<Faculty>> GetFacultiesByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        string? universityId = null,
        string? status = null,
        int page = 1, 
        int pageSize = 50)
    {
        var queryable = _context.Faculties
            .Include(f => f.University)
                .ThenInclude(u => u.Institution)
            .Include(f => f.Departments)
            .Include(f => f.Majors)
            .Where(f => f.University.InstitutionId == institutionId)
            .AsQueryable();

        queryable = ApplyFilters(queryable, query, universityId, null, status);

        return await queryable
            .OrderBy(f => f.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetFacultiesByInstitutionCountAsync(
        Guid institutionId,
        string? query = null,
        string? universityId = null,
        string? status = null)
    {
        var queryable = _context.Faculties
            .Where(f => f.University.InstitutionId == institutionId)
            .AsQueryable();
        
        queryable = ApplyFilters(queryable, query, universityId, null, status);
        return await queryable.CountAsync();
    }

    // Helper method to apply filters
    private IQueryable<Faculty> ApplyFilters(
        IQueryable<Faculty> queryable,
        string? query,
        string? universityId,
        string? institutionId,
        string? status)
    {
        // Text search across name, code, and descriptions
        if (!string.IsNullOrEmpty(query))
        {
            var searchTerm = query.ToLower();
            queryable = queryable.Where(f => 
                (f.Name != null && f.Name.ToLower().Contains(searchTerm)) ||
                (f.Code != null && f.Code.ToLower().Contains(searchTerm)) ||
                (f.ShortDescription != null && f.ShortDescription.ToLower().Contains(searchTerm)) ||
                (f.DetailedDescription != null && f.DetailedDescription.ToLower().Contains(searchTerm)));
        }

        // University filter
        if (!string.IsNullOrEmpty(universityId) && Guid.TryParse(universityId, out var uniId))
        {
            queryable = queryable.Where(f => f.UniversityId == uniId);
        }

        // Institution filter (only apply if not already filtered by institution)
        if (!string.IsNullOrEmpty(institutionId) && Guid.TryParse(institutionId, out var instId))
        {
            queryable = queryable.Where(f => f.University.InstitutionId == instId);
        }

        // Status filter
        if (!string.IsNullOrEmpty(status) &&
            Enum.TryParse<FacultyStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            queryable = queryable.Where(f => f.Status == parsedStatus);
        }

        return queryable;
    }

    #endregion

    #region Validation and lookup methods

    public async Task<Faculty?> GetByNameAndUniversityAsync(string name, Guid universityId)
    {
        return await _context.Faculties
            .FirstOrDefaultAsync(f =>
                f.Name.ToLower() == name.ToLower() &&
                f.UniversityId == universityId);
    }

    public async Task<Faculty?> GetByCodeAndUniversityAsync(string code, Guid universityId)
    {
        return await _context.Faculties
            .FirstOrDefaultAsync(f =>
                f.Code != null &&
                f.Code.ToLower() == code.ToLower() &&
                f.UniversityId == universityId);
    }

    #endregion

    #region Department-related methods

    public async Task<IEnumerable<Department>> GetDepartmentsByFacultyIdAsync(Guid facultyId)
    {
        var faculty = await _context.Faculties
            .Include(f => f.Departments)
            .FirstOrDefaultAsync(f => f.Id == facultyId);

        return faculty?.Departments ?? Enumerable.Empty<Department>();
    }

    #endregion

    #region Major-related methods

    public async Task<IEnumerable<Major>> GetMajorsByFacultyIdAsync(Guid facultyId)
    {
        var faculty = await _context.Faculties
            .Include(f => f.Majors)
            .FirstOrDefaultAsync(f => f.Id == facultyId);

        return faculty?.Majors ?? Enumerable.Empty<Major>();
    }

    #endregion

    #region Bulk operations

    public async Task<IEnumerable<Faculty>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _context.Faculties
            .Include(f => f.University)
                .ThenInclude(u => u.Institution)
            .Include(f => f.Departments)
            .Include(f => f.Majors)
            .Where(f => ids.Contains(f.Id))
            .ToListAsync();
    }

    #endregion

    #region Combined operations

    public async Task<FacultyDepartmentsData> GetFacultyDepartmentsWithValidationAsync(Guid facultyId)
    {
        var result = new FacultyDepartmentsData();

        var faculty = await _context.Faculties
            .AsNoTracking()
            .Include(f => f.Departments)
            .FirstOrDefaultAsync(f => f.Id == facultyId);

        if (faculty == null)
        {
            result.FacultyNotFound = true;
            return result;
        }

        result.FacultyName = faculty.Name;
        result.Departments = faculty.Departments.ToList();
        return result;
    }

    public async Task<FacultyMajorsData> GetFacultyMajorsWithValidationAsync(Guid facultyId)
    {
        var result = new FacultyMajorsData();

        var faculty = await _context.Faculties
            .AsNoTracking()
            .Include(f => f.Majors)
            .FirstOrDefaultAsync(f => f.Id == facultyId);

        if (faculty == null)
        {
            result.FacultyNotFound = true;
            return result;
        }

        result.FacultyName = faculty.Name;
        result.Majors = faculty.Majors.ToList();
        return result;
    }

    #endregion
}
