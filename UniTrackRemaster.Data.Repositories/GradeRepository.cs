using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Repositories;

public class GradeRepository : Repository<Grade>, IGradeRepository
{
    public GradeRepository(UniTrackDbContext context) : base(context) { }

    #region Basic CRUD operations

    public async Task<Grade?> GetByIdAsync(Guid id)
    {
        return await _context.Grades
            .Include(g => g.Students)
            .Include(g => g.Teachers)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Grade?> GetByIdWithRelationsAsync(Guid id)
    {
        return await _context.Grades
            .Include(g => g.Students)
                .ThenInclude(s => s.User)
            .Include(g => g.Teachers)
                .ThenInclude(t => t.User)
            .Include(g => g.HomeRoomTeacher)
            .Include(g => g.Institution)
            .Include(g => g.AcademicYear)
            .Include(g => g.Subjects)
            .FirstOrDefaultAsync(g => g.Id == id);
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

    #endregion

    #region Basic count and exists methods

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Grades.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Grades.AnyAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid institutionId, CancellationToken cancellationToken = default)
    {
        return await _context.Grades.AnyAsync(g => g.Name == name && g.InstitutionId == institutionId, cancellationToken);
    }

    public async Task<bool> HasStudentsAsync(Guid gradeId, CancellationToken cancellationToken = default)
    {
        return await _context.Grades
            .AnyAsync(g => g.Id == gradeId && g.Students.Any(), cancellationToken);
    }

    #endregion

    #region Non-paginated methods (for calculations and internal operations)

    public async Task<IEnumerable<Grade>> GetAllAsync()
    {
        return await _context.Grades
            .Include(g => g.Students)
            .Include(g => g.Teachers)
            .ToListAsync();
    }

    public async Task<IEnumerable<Grade>> GetAllWithRelationsAsync()
    {
        return await _context.Grades
            .Include(g => g.Students)
                .ThenInclude(s => s.User)
            .Include(g => g.Teachers)
                .ThenInclude(t => t.User)
            .Include(g => g.HomeRoomTeacher)
            .Include(g => g.Institution)
            .Include(g => g.AcademicYear)
            .ToListAsync();
    }

    public async Task<IEnumerable<Grade>> GetByInstitutionAsync(Guid institutionId)
    {
        return await _context.Grades
            .Include(g => g.Students)
            .Include(g => g.Teachers)
            .Where(g => g.InstitutionId == institutionId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Grade>> GetByInstitutionWithRelationsAsync(Guid institutionId)
    {
        return await _context.Grades
            .Include(g => g.Students)
                .ThenInclude(s => s.User)
            .Include(g => g.Teachers)
                .ThenInclude(t => t.User)
            .Include(g => g.HomeRoomTeacher)
            .Include(g => g.Institution)
            .Include(g => g.AcademicYear)
            .Where(g => g.InstitutionId == institutionId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Grade>> GetByAcademicYearAsync(Guid academicYearId)
    {
        return await _context.Grades
            .Include(g => g.Students)
            .Include(g => g.Teachers)
            .Where(g => g.AcademicYearId == academicYearId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Grade>> GetByAcademicYearWithRelationsAsync(Guid academicYearId)
    {
        return await _context.Grades
            .Include(g => g.Students)
                .ThenInclude(s => s.User)
            .Include(g => g.Teachers)
                .ThenInclude(t => t.User)
            .Include(g => g.HomeRoomTeacher)
            .Include(g => g.Institution)
            .Include(g => g.AcademicYear)
            .Where(g => g.AcademicYearId == academicYearId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Grade>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        searchTerm = searchTerm.ToLower();

        return await _context.Grades
            .Include(g => g.Students)
            .Include(g => g.Teachers)
            .Where(g => g.Name.ToLower().Contains(searchTerm))
            .ToListAsync();
    }

    public async Task<IEnumerable<Grade>> SearchWithRelationsAsync(string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
            return await GetAllWithRelationsAsync();

        return await _context.Grades
            .Include(g => g.Students)
                .ThenInclude(s => s.User)
            .Include(g => g.Teachers)
                .ThenInclude(t => t.User)
            .Include(g => g.HomeRoomTeacher)
            .Include(g => g.Institution)
            .Include(g => g.AcademicYear)
            .Where(g => g.Name.Contains(searchTerm))
            .ToListAsync();
    }

    #endregion

    #region Paginated methods with filtering (for API endpoints)

    public async Task<List<Grade>> GetAllWithRelationsAsync(
        string? query = null,
        string? institutionId = null,
        string? academicYearId = null,
        bool? hasHomeRoomTeacher = null,
        int page = 1, 
        int pageSize = 50)
    {
        var queryable = _context.Grades
            .Include(g => g.Students)
                .ThenInclude(s => s.User)
            .Include(g => g.Teachers)
                .ThenInclude(t => t.User)
            .Include(g => g.HomeRoomTeacher)
            .Include(g => g.Institution)
            .Include(g => g.AcademicYear)
            .AsQueryable();

        queryable = ApplyFilters(queryable, query, institutionId, academicYearId, hasHomeRoomTeacher);

        return await queryable
            .OrderBy(g => g.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(
        string? query = null,
        string? institutionId = null,
        string? academicYearId = null,
        bool? hasHomeRoomTeacher = null)
    {
        var queryable = _context.Grades.AsQueryable();
        queryable = ApplyFilters(queryable, query, institutionId, academicYearId, hasHomeRoomTeacher);
        return await queryable.CountAsync();
    }

    public async Task<List<Grade>> GetGradesByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        string? academicYearId = null,
        bool? hasHomeRoomTeacher = null,
        int page = 1, 
        int pageSize = 50)
    {
        var queryable = _context.Grades
            .Include(g => g.Students)
                .ThenInclude(s => s.User)
            .Include(g => g.Teachers)
                .ThenInclude(t => t.User)
            .Include(g => g.HomeRoomTeacher)
            .Include(g => g.Institution)
            .Include(g => g.AcademicYear)
            .Where(g => g.InstitutionId == institutionId)
            .AsQueryable();

        queryable = ApplyFilters(queryable, query, null, academicYearId, hasHomeRoomTeacher);

        return await queryable
            .OrderBy(g => g.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetGradesByInstitutionCountAsync(
        Guid institutionId,
        string? query = null,
        string? academicYearId = null,
        bool? hasHomeRoomTeacher = null)
    {
        var queryable = _context.Grades
            .Where(g => g.InstitutionId == institutionId)
            .AsQueryable();
        
        queryable = ApplyFilters(queryable, query, null, academicYearId, hasHomeRoomTeacher);
        return await queryable.CountAsync();
    }

    // Helper method to apply filters
    private IQueryable<Grade> ApplyFilters(
        IQueryable<Grade> queryable,
        string? query,
        string? institutionId,
        string? academicYearId,
        bool? hasHomeRoomTeacher)
    {
        // Text search on grade name
        if (!string.IsNullOrEmpty(query))
        {
            var searchTerm = query.ToLower();
            queryable = queryable.Where(g => 
                g.Name != null && g.Name.ToLower().Contains(searchTerm));
        }

        // Institution filter (only apply if not already filtered by institution)
        if (!string.IsNullOrEmpty(institutionId) && Guid.TryParse(institutionId, out var instId))
        {
            queryable = queryable.Where(g => g.InstitutionId == instId);
        }

        // Academic year filter
        if (!string.IsNullOrEmpty(academicYearId) && Guid.TryParse(academicYearId, out var yearId))
        {
            queryable = queryable.Where(g => g.AcademicYearId == yearId);
        }

        return queryable;
    }

    #endregion

    #region Teacher-related methods

    public async Task<IEnumerable<Grade>> GetByTeacherIdAsync(Guid teacherId)
    {
        return await _context.Grades
            .Include(g => g.Students)
            .Include(g => g.Teachers)
            .Where(g => g.Teachers.Any(t => t.Id == teacherId) || g.HomeRoomTeacherId == teacherId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Grade>> GetByHomeRoomTeacherIdAsync(Guid teacherId)
    {
        return await _context.Grades
            .Include(g => g.Students)
            .Include(g => g.Teachers)
            .Where(g => g.HomeRoomTeacherId == teacherId)
            .ToListAsync();
    }

    public async Task AssignTeacherAsync(Guid gradeId, Guid teacherId)
    {
        var grade = await _context.Grades
            .Include(g => g.Teachers)
            .FirstOrDefaultAsync(g => g.Id == gradeId);

        var teacher = await _context.Teachers
            .FirstOrDefaultAsync(t => t.Id == teacherId);

        if (grade != null && teacher != null)
        {
            grade.Teachers.Add(teacher);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> RemoveTeacherAsync(Guid gradeId, Guid teacherId)
    {
        var grade = await _context.Grades
            .Include(g => g.Teachers)
            .FirstOrDefaultAsync(g => g.Id == gradeId);

        if (grade == null)
            return false;

        var teacher = grade.Teachers.FirstOrDefault(t => t.Id == teacherId);
        if (teacher == null)
            return false;

        grade.Teachers.Remove(teacher);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Student-related methods

    public async Task<IEnumerable<Student>> GetStudentsByGradeIdAsync(Guid gradeId)
    {
        var grade = await _context.Grades
            .Include(g => g.Students)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(g => g.Id == gradeId);

        return grade?.Students ?? Enumerable.Empty<Student>();
    }

    #endregion

    #region Bulk operations

    public async Task<IEnumerable<Grade>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _context.Grades
            .Include(g => g.Institution)
            .Include(g => g.AcademicYear)
            .Include(g => g.Students)
            .Include(g => g.Teachers)
            .Where(g => ids.Contains(g.Id))
            .ToListAsync();
    }

    #endregion

    #region Combined operations

    public async Task<TeacherAssignmentValidation> ValidateTeacherAssignmentAsync(Guid gradeId, Guid teacherId)
    {
        var result = new TeacherAssignmentValidation();

        var grade = await _context.Grades
            .AsNoTracking()
            .Include(g => g.Teachers)
            .FirstOrDefaultAsync(g => g.Id == gradeId);

        if (grade == null)
        {
            result.GradeNotFound = true;
            return result;
        }

        var teacherExists = await _context.Teachers
            .AnyAsync(t => t.Id == teacherId);

        result.TeacherNotFound = !teacherExists;

        if (teacherExists)
        {
            result.AlreadyAssigned = grade.Teachers.Any(t => t.Id == teacherId);
        }

        return result;
    }

    public async Task<GradeTeachersData> GetGradeTeachersWithValidationAsync(Guid gradeId)
    {
        var result = new GradeTeachersData();

        var grade = await _context.Grades
            .AsNoTracking()
            .Include(g => g.Teachers)
                .ThenInclude(t => t.User)
            .Include(g => g.HomeRoomTeacher)
            .FirstOrDefaultAsync(g => g.Id == gradeId);

        if (grade == null)
        {
            result.GradeNotFound = true;
            return result;
        }

        result.GradeName = grade.Name;
        result.Teachers = grade.Teachers.ToList();
        return result;
    }

    public async Task<GradeStudentsData> GetGradeStudentsWithValidationAsync(Guid gradeId)
    {
        var result = new GradeStudentsData();

        var grade = await _context.Grades
            .AsNoTracking()
            .Include(g => g.Students)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(g => g.Id == gradeId);

        if (grade == null)
        {
            result.GradeNotFound = true;
            return result;
        }

        result.GradeName = grade.Name;
        result.Students = grade.Students.ToList();
        return result;
    }

    #endregion
}
