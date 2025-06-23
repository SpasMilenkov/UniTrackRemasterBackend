using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Repositories;

public class DepartmentRepository : Repository<Department>, IDepartmentRepository
{
    public DepartmentRepository(UniTrackDbContext context) : base(context) { }

    #region Basic CRUD operations

    public async Task<Department?> GetByIdAsync(Guid id)
    {
        return await _context.Departments
            .Include(d => d.Faculty)
            .Include(d => d.Teachers)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Department?> GetByIdWithRelationsAsync(Guid id)
    {
        return await _context.Departments
            .Include(d => d.Faculty)
                .ThenInclude(f => f.University)
                    .ThenInclude(u => u.Institution)
            .Include(d => d.Teachers)
                .ThenInclude(t => t.User)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Department> CreateAsync(Department department)
    {
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task UpdateAsync(Department department)
    {
        _context.Departments.Update(department);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var department = await GetByIdAsync(id);
        if (department != null)
        {
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Basic count and exists methods

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Departments.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Departments.AnyAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid facultyId, CancellationToken cancellationToken = default)
    {
        return await _context.Departments.AnyAsync(d => d.Name == name && d.FacultyId == facultyId, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, Guid facultyId, CancellationToken cancellationToken = default)
    {
        return await _context.Departments.AnyAsync(d => d.Code == code && d.FacultyId == facultyId, cancellationToken);
    }

    public async Task<bool> HasTeachersAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .AnyAsync(d => d.Id == departmentId && d.Teachers.Any(), cancellationToken);
    }

    #endregion

    #region Non-paginated methods (for calculations and internal operations)

    public async Task<IEnumerable<Department>> GetAllAsync()
    {
        return await _context.Departments
            .Include(d => d.Faculty)
            .Include(d => d.Teachers)
            .ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetAllWithRelationsAsync()
    {
        return await _context.Departments
            .Include(d => d.Faculty)
                .ThenInclude(f => f.University)
                    .ThenInclude(u => u.Institution)
            .Include(d => d.Teachers)
                .ThenInclude(t => t.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetByFacultyAsync(Guid facultyId)
    {
        return await _context.Departments
            .Include(d => d.Faculty)
            .Include(d => d.Teachers)
            .Where(d => d.FacultyId == facultyId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetByFacultyWithRelationsAsync(Guid facultyId)
    {
        return await _context.Departments
            .Include(d => d.Faculty)
                .ThenInclude(f => f.University)
                    .ThenInclude(u => u.Institution)
            .Include(d => d.Teachers)
                .ThenInclude(t => t.User)
            .Where(d => d.FacultyId == facultyId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetByInstitutionAsync(Guid institutionId)
    {
        return await _context.Departments
            .Include(d => d.Faculty)
            .Include(d => d.Teachers)
            .Where(d => d.Faculty.University.InstitutionId == institutionId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetByInstitutionWithRelationsAsync(Guid institutionId)
    {
        return await _context.Departments
            .Include(d => d.Faculty)
                .ThenInclude(f => f.University)
                    .ThenInclude(u => u.Institution)
            .Include(d => d.Teachers)
                .ThenInclude(t => t.User)
            .Where(d => d.Faculty.University.InstitutionId == institutionId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Department>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        searchTerm = searchTerm.ToLower();

        return await _context.Departments
            .Include(d => d.Faculty)
            .Include(d => d.Teachers)
            .Where(d => d.Name.ToLower().Contains(searchTerm) ||
                   (d.Code != null && d.Code.ToLower().Contains(searchTerm)) ||
                   (d.Description != null && d.Description.ToLower().Contains(searchTerm)) ||
                   (d.Location != null && d.Location.ToLower().Contains(searchTerm)))
            .ToListAsync();
    }

    public async Task<IEnumerable<Department>> SearchWithRelationsAsync(string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
            return await GetAllWithRelationsAsync();

        return await _context.Departments
            .Include(d => d.Faculty)
                .ThenInclude(f => f.University)
                    .ThenInclude(u => u.Institution)
            .Include(d => d.Teachers)
                .ThenInclude(t => t.User)
            .Where(d => d.Name.Contains(searchTerm) ||
                   (d.Code != null && d.Code.Contains(searchTerm)) ||
                   (d.Description != null && d.Description.Contains(searchTerm)) ||
                   (d.Location != null && d.Location.Contains(searchTerm)))
            .ToListAsync();
    }

    #endregion

    #region Paginated methods with filtering (for API endpoints)

    public async Task<List<Department>> GetAllWithRelationsAsync(
        string? query = null,
        string? facultyId = null,
        string? institutionId = null,
        string? type = null,
        string? status = null,
        int page = 1, 
        int pageSize = 50)
    {
        var queryable = _context.Departments
            .Include(d => d.Faculty)
                .ThenInclude(f => f.University)
                    .ThenInclude(u => u.Institution)
            .Include(d => d.Teachers)
                .ThenInclude(t => t.User)
            .AsQueryable();

        queryable = ApplyFilters(queryable, query, facultyId, institutionId, type, status);

        return await queryable
            .OrderBy(d => d.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(
        string? query = null,
        string? facultyId = null,
        string? institutionId = null,
        string? type = null,
        string? status = null)
    {
        var queryable = _context.Departments.AsQueryable();
        queryable = ApplyFilters(queryable, query, facultyId, institutionId, type, status);
        return await queryable.CountAsync();
    }

    public async Task<List<Department>> GetDepartmentsByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        string? facultyId = null,
        string? type = null,
        string? status = null,
        int page = 1, 
        int pageSize = 50)
    {
        var queryable = _context.Departments
            .Include(d => d.Faculty)
                .ThenInclude(f => f.University)
                    .ThenInclude(u => u.Institution)
            .Include(d => d.Teachers)
                .ThenInclude(t => t.User)
            .Where(d => d.Faculty.University.InstitutionId == institutionId)
            .AsQueryable();

        queryable = ApplyFilters(queryable, query, facultyId, null, type, status);

        return await queryable
            .OrderBy(d => d.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetDepartmentsByInstitutionCountAsync(
        Guid institutionId,
        string? query = null,
        string? facultyId = null,
        string? type = null,
        string? status = null)
    {
        var queryable = _context.Departments
            .Where(d => d.Faculty.University.InstitutionId == institutionId)
            .AsQueryable();
        
        queryable = ApplyFilters(queryable, query, facultyId, null, type, status);
        return await queryable.CountAsync();
    }

    // Helper method to apply filters
    private IQueryable<Department> ApplyFilters(
        IQueryable<Department> queryable,
        string? query,
        string? facultyId,
        string? institutionId,
        string? type,
        string? status)
    {
        // Text search across name, code, description, and location
        if (!string.IsNullOrEmpty(query))
        {
            var searchTerm = query.ToLower();
            queryable = queryable.Where(d => 
                (d.Name != null && d.Name.ToLower().Contains(searchTerm)) ||
                (d.Code != null && d.Code.ToLower().Contains(searchTerm)) ||
                (d.Description != null && d.Description.ToLower().Contains(searchTerm)) ||
                (d.Location != null && d.Location.ToLower().Contains(searchTerm)));
        }

        // Faculty filter
        if (!string.IsNullOrEmpty(facultyId) && Guid.TryParse(facultyId, out var facId))
        {
            queryable = queryable.Where(d => d.FacultyId == facId);
        }

        // Institution filter (only apply if not already filtered by institution)
        if (!string.IsNullOrEmpty(institutionId) && Guid.TryParse(institutionId, out var instId))
        {
            queryable = queryable.Where(d => d.Faculty.University.InstitutionId == instId);
        }

        // Type filter
        if (!string.IsNullOrEmpty(type) &&
            Enum.TryParse<DepartmentType>(type, ignoreCase: true, out var parsedType))
        {
            queryable = queryable.Where(d => d.Type == parsedType);
        }

        // Status filter
        if (!string.IsNullOrEmpty(status) &&
            Enum.TryParse<DepartmentStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            queryable = queryable.Where(d => d.Status == parsedStatus);
        }

        return queryable;
    }

    #endregion

    #region Validation and lookup methods

    public async Task<Department?> GetByNameAndFacultyAsync(string name, Guid facultyId)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(d =>
                d.Name.ToLower() == name.ToLower() &&
                d.FacultyId == facultyId);
    }

    public async Task<Department?> GetByCodeAndFacultyAsync(string code, Guid facultyId)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(d =>
                d.Code != null &&
                d.Code.ToLower() == code.ToLower() &&
                d.FacultyId == facultyId);
    }

    #endregion

    #region Teacher-related methods

    public async Task<IEnumerable<Teacher>> GetTeachersByDepartmentIdAsync(Guid departmentId)
    {
        var department = await _context.Departments
            .Include(d => d.Teachers)
                .ThenInclude(t => t.User)
            .FirstOrDefaultAsync(d => d.Id == departmentId);

        return department?.Teachers ?? Enumerable.Empty<Teacher>();
    }

    public async Task AssignTeacherAsync(Guid departmentId, Guid teacherId)
    {
        var department = await _context.Departments
            .Include(d => d.Teachers)
            .FirstOrDefaultAsync(d => d.Id == departmentId);

        var teacher = await _context.Teachers
            .FirstOrDefaultAsync(t => t.Id == teacherId);

        if (department != null && teacher != null)
        {
            department.Teachers.Add(teacher);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> RemoveTeacherAsync(Guid departmentId, Guid teacherId)
    {
        var department = await _context.Departments
            .Include(d => d.Teachers)
            .FirstOrDefaultAsync(d => d.Id == departmentId);

        if (department == null)
            return false;

        var teacher = department.Teachers.FirstOrDefault(t => t.Id == teacherId);
        if (teacher == null)
            return false;

        department.Teachers.Remove(teacher);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Bulk operations

    public async Task<IEnumerable<Department>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _context.Departments
            .Include(d => d.Faculty)
                .ThenInclude(f => f.University)
                    .ThenInclude(u => u.Institution)
            .Include(d => d.Teachers)
            .Where(d => ids.Contains(d.Id))
            .ToListAsync();
    }

    #endregion

    #region Combined operations

    public async Task<TeacherAssignmentValidation> ValidateTeacherAssignmentAsync(Guid departmentId, Guid teacherId)
    {
        var result = new TeacherAssignmentValidation();

        var department = await _context.Departments
            .AsNoTracking()
            .Include(d => d.Teachers)
            .FirstOrDefaultAsync(d => d.Id == departmentId);

        if (department == null)
        {
            return result;
        }

        var teacherExists = await _context.Teachers
            .AnyAsync(t => t.Id == teacherId);

        result.TeacherNotFound = !teacherExists;

        if (teacherExists)
        {
            result.AlreadyAssigned = department.Teachers.Any(t => t.Id == teacherId);
        }

        return result;
    }

    public async Task<DepartmentTeachersData> GetDepartmentTeachersWithValidationAsync(Guid departmentId)
    {
        var result = new DepartmentTeachersData();

        var department = await _context.Departments
            .AsNoTracking()
            .Include(d => d.Teachers)
                .ThenInclude(t => t.User)
            .FirstOrDefaultAsync(d => d.Id == departmentId);

        if (department == null)
        {
            result.DepartmentNotFound = true;
            return result;
        }

        result.DepartmentName = department.Name;
        result.Teachers = department.Teachers.ToList();
        return result;
    }

    #endregion
}