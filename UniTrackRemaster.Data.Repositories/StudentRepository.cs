using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Repositories;

public class StudentRepository(UniTrackDbContext context) : Repository<Student>(context), IStudentRepository
{
    

    #region Basic CRUD Operations

    public async Task<Student?> GetByIdAsync(Guid id)
    {
        return await context.Students
            .Include(s => s.User)
            .Include(s => s.School)
            .ThenInclude(s => s.Institution)
            .Include(s => s.University)
            .ThenInclude(u => u.Institution)
            .Include(s => s.Grade)
            .Include(s => s.Marks)
            .Include(s => s.AttendanceRecords)
            .Include(s => s.Electives)
            .FirstOrDefaultAsync(s => s.Id == id);
    }


    public async Task<Student?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Grade)
            .Include(s => s.Major)
            .FirstOrDefaultAsync(s => s.UserId == userId);
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
            .Include(s => s.Major)
            .Where(s => s.UniversityId == universityId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetByGradeIdAsync(Guid gradeId)
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
        var student = await _context.Students.FindAsync(id);
        if (student != null)
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Search and Pagination

    public async Task<(IEnumerable<Student> Students, int TotalCount)> SearchWithPaginationAsync(
        string? query = null,
        Guid? gradeId = null,
        Guid? institutionId = null,
        int pageNumber = 1,
        int pageSize = 20,
        string sortBy = "FirstName",
        bool ascending = true)
    {
        var queryable = _context.Students
            .Include(s => s.User)
            .Include(s => s.Grade)
            .AsQueryable();

        if (!string.IsNullOrEmpty(query))
        {
            queryable = queryable.Where(s => 
                s.User!.FirstName.Contains(query) || 
                s.User!.LastName.Contains(query) ||
                s.User!.Email!.Contains(query));
        }

        if (gradeId.HasValue)
            queryable = queryable.Where(s => s.GradeId == gradeId.Value);

        if (institutionId.HasValue)
            queryable = queryable.Where(s => 
                s.SchoolId == institutionId.Value || 
                s.UniversityId == institutionId.Value);

        var totalCount = await queryable.CountAsync();

        // Apply sorting
        queryable = sortBy.ToLower() switch
        {
            "lastname" => ascending ? queryable.OrderBy(s => s.User!.LastName) : queryable.OrderByDescending(s => s.User!.LastName),
            "email" => ascending ? queryable.OrderBy(s => s.User!.Email) : queryable.OrderByDescending(s => s.User!.Email),
            "createdat" => ascending ? queryable.OrderBy(s => s.CreatedAt) : queryable.OrderByDescending(s => s.CreatedAt),
            _ => ascending ? queryable.OrderBy(s => s.User!.FirstName) : queryable.OrderByDescending(s => s.User!.FirstName)
        };

        var students = await queryable
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (students, totalCount);
    }

    #endregion

    #region Invitation System

    public async Task<IEnumerable<Student>> GetPendingByUserIdAsync(Guid userId)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Grade)
            .Where(s => s.UserId == userId && s.Status == ProfileStatus.Pending)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetByInstitutionAsync(Guid institutionId, ProfileStatus? status = null)
    {
        var query = _context.Students
            .Include(s => s.User)
            .Include(s => s.Grade)
            .Where(s => s.SchoolId == institutionId || s.UniversityId == institutionId);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetByStatusAsync(ProfileStatus status)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Grade)
            .Where(s => s.Status == status)
            .ToListAsync();
    }

    #endregion

    #region Performance-Optimized Analytics Methods

    /// <summary>
    /// OPTIMIZED: Single query with proper joins to get all dashboard data
    /// </summary>
    public async Task<(
        List<Mark> Marks,
        Dictionary<Guid, Subject> Subjects,
        Dictionary<Guid, Teacher> Teachers,
        List<Student> Classmates,
        Dictionary<Guid, Semester> Semesters
        )> GetStudentDashboardDataAsync(Guid studentId, AcademicYear? academicYear = null, Semester? semester = null)
    {
        // Get student first to determine grade
        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == studentId);
        
        if (student == null)
            return (new(), new(), new(), new(), new());

        // Build marks query with all necessary includes
        var marksQuery = _context.Marks
            .Include(m => m.Subject)
            .Include(m => m.Teacher)
                .ThenInclude(t => t.User)
            .Include(m => m.Semester)
                .ThenInclude(s => s.AcademicYear)
            .Where(m => m.StudentId == studentId);

        // Apply filters
        if (academicYear != null)
            marksQuery = marksQuery.Where(m => m.Semester!.AcademicYearId == academicYear.Id);
            
        if (semester != null)
            marksQuery = marksQuery.Where(m => m.SemesterId == semester.Id);

        var marks = await marksQuery.ToListAsync();

        // Extract subjects, teachers, and semesters from loaded marks (no additional queries)
        var subjects = marks
            .Where(m => m.Subject != null)
            .GroupBy(m => m.Subject!.Id)
            .ToDictionary(g => g.Key, g => g.First().Subject!);

        var teachers = marks
            .Where(m => m.Teacher != null)
            .GroupBy(m => m.Teacher!.Id)
            .ToDictionary(g => g.Key, g => g.First().Teacher!);

        var semesters = marks
            .Where(m => m.Semester != null)
            .GroupBy(m => m.Semester!.Id)
            .ToDictionary(g => g.Key, g => g.First().Semester!);

        // Get classmates in single query
        var classmates = await _context.Students
            .Include(s => s.User)
            .Where(s => s.GradeId == student.GradeId && s.Id != studentId)
            .ToListAsync();

        return (marks, subjects, teachers, classmates, semesters);
    }

    /// <summary>
    /// OPTIMIZED: Single query with joins instead of separate lookups
    /// </summary>
    public async Task<(List<Mark> Marks, Dictionary<Guid, Subject> Subjects, Dictionary<Guid, Semester> Semesters)>
        GetStudentMarksWithSubjectsAsync(
            Guid studentId, 
            AcademicYear? academicYear = null, 
            Semester? semester = null,
            CancellationToken cancellationToken = default)
    {
        var query = _context.Marks
            .Include(m => m.Subject)
            .Include(m => m.Semester)
                .ThenInclude(s => s.AcademicYear)
            .Where(m => m.StudentId == studentId);

        if (academicYear != null)
            query = query.Where(m => m.Semester!.AcademicYearId == academicYear.Id);
            
        if (semester != null)
            query = query.Where(m => m.SemesterId == semester.Id);

        var marks = await query.ToListAsync(cancellationToken);

        var subjects = marks
            .Where(m => m.Subject != null)
            .GroupBy(m => m.Subject!.Id)
            .ToDictionary(g => g.Key, g => g.First().Subject!);

        var semesters = marks
            .Where(m => m.Semester != null)
            .GroupBy(m => m.Semester!.Id)
            .ToDictionary(g => g.Key, g => g.First().Semester!);

        return (marks, subjects, semesters);
    }

    /// <summary>
    /// OPTIMIZED: Single query with grouping instead of multiple queries
    /// </summary>
    public async Task<Dictionary<string, (List<Mark> Marks, Semester Semester)>> GetStudentMarksBySemestersAsync(
        Guid studentId, 
        Guid institutionId, 
        CancellationToken cancellationToken = default)
    {
        var marksWithSemesters = await _context.Marks
            .Include(m => m.Semester)
                .ThenInclude(s => s.AcademicYear)
            .Where(m => m.StudentId == studentId && 
                       m.Semester != null && 
                       m.Semester.AcademicYear!.InstitutionId == institutionId)
            .ToListAsync(cancellationToken);

        return marksWithSemesters
            .GroupBy(m => $"{m.Semester!.Type} {m.Semester.AcademicYear!.Name}")
            .ToDictionary(
                g => g.Key,
                g => (g.ToList(), g.First().Semester!)
            );
    }

    /// <summary>
    /// OPTIMIZED: Single query for attendance data
    /// </summary>
    public async Task<List<Absence>> GetStudentAttendanceDataAsync(
        Guid studentId, 
        AcademicYear? academicYear = null, 
        Semester? semester = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Absences
            .Where(a => a.StudentId == studentId);

        if (academicYear != null)
        {
            query = query.Where(a => a.Semester != null && a.Semester.AcademicYearId == academicYear.Id);
        }

        if (semester != null)
        {
            query = query.Where(a => a.SemesterId == semester.Id);
        }

        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// OPTIMIZED: Aggregated query instead of loading all individual marks
    /// </summary>
    public async Task<Dictionary<Guid, List<(Guid StudentId, List<Mark> Marks)>>> GetClassPerformanceDataAsync(
        Guid gradeId, 
        AcademicYear? academicYear = null, 
        Semester? semester = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Marks
            .Include(m => m.Student)
            .Where(m => m.Student!.GradeId == gradeId);

        if (academicYear != null)
            query = query.Where(m => m.Semester!.AcademicYearId == academicYear.Id);
            
        if (semester != null)
            query = query.Where(m => m.SemesterId == semester.Id);

        var classMarks = await query.ToListAsync(cancellationToken);

        return classMarks
            .GroupBy(m => m.SubjectId)
            .ToDictionary(
                subjectGroup => subjectGroup.Key,
                subjectGroup => subjectGroup
                    .GroupBy(m => m.StudentId)
                    .Select(studentGroup => (studentGroup.Key, studentGroup.ToList()))
                    .ToList()
            );
    }

    /// <summary>
    /// OPTIMIZED: Single query with grouping
    /// </summary>
    public async Task<Dictionary<Guid, List<(Semester Semester, List<Mark> Marks)>>> GetAcademicYearBreakdownAsync(
        Guid studentId, 
        Guid academicYearId, 
        CancellationToken cancellationToken = default)
    {
        var marks = await _context.Marks
            .Include(m => m.Semester)
            .Where(m => m.StudentId == studentId && 
                       m.Semester != null && 
                       m.Semester.AcademicYearId == academicYearId)
            .ToListAsync(cancellationToken);

        return marks
            .GroupBy(m => m.SubjectId)
            .ToDictionary(
                subjectGroup => subjectGroup.Key,
                subjectGroup => subjectGroup
                    .GroupBy(m => m.Semester!)
                    .Select(semesterGroup => (semesterGroup.Key, semesterGroup.ToList()))
                    .ToList()
            );
    }

    #endregion
}
