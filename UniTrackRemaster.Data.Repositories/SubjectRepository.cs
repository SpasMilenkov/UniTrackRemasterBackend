using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Repositories;


// SubjectRepository.cs - Implementation
public class SubjectRepository : Repository<Subject>, ISubjectRepository
{
    public SubjectRepository(UniTrackDbContext context) : base(context) { }

    #region Basic CRUD operations

    public async Task<Subject?> GetByIdAsync(Guid id)
    {
        return await _context.Subjects
            .Include(s => s.Teachers)
            .Include(s => s.Grades)
            .ThenInclude(g => g.Teachers)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Subject?> GetByIdWithRelationsAsync(Guid id)
    {
        return await _context.Subjects
            .Include(s => s.Teachers)
            .Include(s => s.Grades)
            .Include(s => s.Department)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Subject> CreateAsync(Subject subject)
    {
        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();
        return subject;
    }

    public async Task UpdateAsync(Subject subject)
    {
        _context.Subjects.Update(subject);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var subject = await GetByIdAsync(id);
        if (subject != null)
        {
            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Basic count and exists methods

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Subjects.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Subjects.AnyAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Subjects.AnyAsync(s => s.Code == code, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Subjects.AnyAsync(s => s.Name == name, cancellationToken);
    }

    #endregion

    #region Non-paginated methods (for calculations and internal operations)

    public async Task<IEnumerable<Subject>> GetAllAsync()
    {
        return await _context.Subjects
            .Include(s => s.Teachers)
            .Include(s => s.Grades)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subject>> GetAllWithRelationsAsync()
    {
        return await _context.Subjects
            .Include(s => s.Teachers)
            .Include(s => s.Grades)
            .Include(s => s.Department)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subject>> GetByDepartmentAsync(Guid departmentId)
    {
        return await _context.Subjects
            .Include(s => s.Teachers)
            .Include(s => s.Grades)
            .Where(s => s.DepartmentId == departmentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subject>> GetByDepartmentWithRelationsAsync(Guid departmentId)
    {
        return await _context.Subjects
            .Include(s => s.Teachers)
            .Include(s => s.Grades)
            .Include(s => s.Department)
            .Where(s => s.DepartmentId == departmentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subject>> GetElectivesAsync(bool activeOnly = true)
    {
        var query = _context.Subjects
            .Include(s => s.Teachers)
            .Include(s => s.Grades)
            .Where(s => s.IsElective);

        if (activeOnly)
        {
            query = query.Where(s => s.MaxStudents == null ||
                s.StudentElectives.Count(se => se.Status == ElectiveStatus.Enrolled) < s.MaxStudents);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Subject>> GetElectivesWithRelationsAsync(bool activeOnly)
    {
        var query = _context.Subjects
            .Include(s => s.Teachers)
            .Include(s => s.Grades)
            .Include(s => s.Department)
            .Where(s => s.IsElective);

        if (activeOnly)
        {
            query = query.Where(s =>
                !s.MaxStudents.HasValue ||
                s.StudentElectives.Count(se => se.Status == ElectiveStatus.Enrolled) < s.MaxStudents.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Subject>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        searchTerm = searchTerm.ToLower();

        return await _context.Subjects
            .Include(s => s.Teachers)
            .Include(s => s.Grades)
            .Where(s => s.Name.ToLower().Contains(searchTerm) ||
                   s.Code.ToLower().Contains(searchTerm) ||
                   s.ShortDescription.ToLower().Contains(searchTerm) ||
                   (s.DetailedDescription != null && s.DetailedDescription.ToLower().Contains(searchTerm)))
            .ToListAsync();
    }

    public async Task<IEnumerable<Subject>> SearchWithRelationsAsync(string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
            return await GetAllWithRelationsAsync();

        return await _context.Subjects
            .Include(s => s.Teachers)
            .Include(s => s.Grades)
            .Include(s => s.Department)
            .Where(s =>
                s.Name.Contains(searchTerm) ||
                s.Code.Contains(searchTerm) ||
                s.ShortDescription.Contains(searchTerm) ||
                s.DetailedDescription.Contains(searchTerm))
            .ToListAsync();
    }

    #endregion

    #region Paginated methods with filtering (for API endpoints)

    public async Task<List<Subject>> GetAllWithRelationsAsync(
        string? query = null,
        string? departmentId = null,
        string? academicLevel = null,
        string? electiveType = null,
        bool? hasLab = null,
        bool? isElective = null,
        int page = 1, 
        int pageSize = 50)
    {
        var queryable = _context.Subjects
            .Include(s => s.Teachers)
            .Include(s => s.Grades)
            .Include(s => s.Department)
            .AsQueryable();

        queryable = ApplyFilters(queryable, query, departmentId, academicLevel, electiveType, hasLab, isElective);

        return await queryable
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(
        string? query = null,
        string? departmentId = null,
        string? academicLevel = null,
        string? electiveType = null,
        bool? hasLab = null,
        bool? isElective = null)
    {
        var queryable = _context.Subjects.AsQueryable();
        queryable = ApplyFilters(queryable, query, departmentId, academicLevel, electiveType, hasLab, isElective);
        return await queryable.CountAsync();
    }

    public async Task<List<Subject>> GetSubjectsByInstitutionAsync(
        Guid institutionId,
        string? query = null,
        string? departmentId = null,
        string? academicLevel = null,
        string? electiveType = null,
        bool? hasLab = null,
        bool? isElective = null,
        int page = 1, 
        int pageSize = 50)
    {
        var queryable = _context.Subjects
            .Include(s => s.Teachers)
            .Include(s => s.Grades)
            .Include(s => s.Department)
            .Where(s => s.InstitutionId == institutionId)
            .AsQueryable();

        queryable = ApplyFilters(queryable, query, departmentId, academicLevel, electiveType, hasLab, isElective);

        return await queryable
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetSubjectsByInstitutionCountAsync(
        Guid institutionId,
        string? query = null,
        string? departmentId = null,
        string? academicLevel = null,
        string? electiveType = null,
        bool? hasLab = null,
        bool? isElective = null)
    {
        var queryable = _context.Subjects
            .Where(s => s.InstitutionId == institutionId)
            .AsQueryable();
        
        queryable = ApplyFilters(queryable, query, departmentId, academicLevel, electiveType, hasLab, isElective);
        return await queryable.CountAsync();
    }

    // Helper method to apply filters
    private IQueryable<Subject> ApplyFilters(
        IQueryable<Subject> queryable,
        string? query,
        string? departmentId,
        string? academicLevel,
        string? electiveType,
        bool? hasLab,
        bool? isElective)
    {
        // Text search across name, code, and description
        if (!string.IsNullOrEmpty(query))
        {
            var searchTerm = query.ToLower();
            queryable = queryable.Where(s => 
                (s.Name != null && s.Name.ToLower().Contains(searchTerm)) ||
                (s.Code != null && s.Code.ToLower().Contains(searchTerm)) ||
                (s.ShortDescription != null && s.ShortDescription.ToLower().Contains(searchTerm)));
        }

        // Department filter
        if (!string.IsNullOrEmpty(departmentId) && Guid.TryParse(departmentId, out var deptId))
        {
            queryable = queryable.Where(s => s.DepartmentId == deptId);
        }

        // Academic level filter
        if (!string.IsNullOrEmpty(academicLevel) &&
            Enum.TryParse<AcademicLevel>(academicLevel, ignoreCase: true, out var parsedLevel))
        {
            queryable = queryable.Where(s => s.AcademicLevel == parsedLevel);
        }

        if (!string.IsNullOrEmpty(electiveType) &&
            Enum.TryParse<ElectiveType>(electiveType, ignoreCase: true, out var parsedType))
        {
            queryable = queryable.Where(s => s.ElectiveType == parsedType);
        }

        // Has lab filter
        if (hasLab.HasValue)
        {
            queryable = queryable.Where(s => s.HasLab == hasLab.Value);
        }

        // Is elective filter
        if (isElective.HasValue)
        {
            queryable = queryable.Where(s => s.IsElective == isElective.Value);
        }

        return queryable;
    }

    #endregion

    #region Teacher-related methods

    public async Task<IEnumerable<Subject>> GetByTeacherIdAsync(Guid teacherId)
    {
        return await _context.Subjects
            .Include(s => s.Teachers)
            .Include(s => s.Grades)
            .Where(s => s.Teachers.Any(t => t.Id == teacherId))
            .ToListAsync();
    }

    public async Task<IEnumerable<Subject>> GetByPrimaryTeacherIdAsync(Guid teacherId)
    {
        return await _context.Subjects
            .Include(s => s.Teachers)
            .Include(s => s.Grades)
            .Where(s => s.PrimaryTeacherId == teacherId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subject>> GetSubjectsByMarksAsync(Guid teacherId)
    {
        return await _context.Marks
            .Include(m => m.Subject)
            .Where(m => m.TeacherId == teacherId)
            .Select(m => m.Subject)
            .Distinct()
            .ToListAsync();
    }

    #endregion

    #region Student-related methods

    public async Task<IEnumerable<Student>> GetStudentsBySubjectIdAsync(Guid subjectId)
    {
        var subject = await _context.Subjects
            .Include(s => s.Grades)
                .ThenInclude(g => g.Students)
                    .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == subjectId);

        if (subject == null)
            return Enumerable.Empty<Student>();

        // Get students from all grades teaching this subject
        var regularStudents = subject.Grades
            .SelectMany(g => g.Students)
            .ToList();

        // Get students enrolled in this subject as an elective
        var electiveStudents = await _context.StudentElectives
            .Include(se => se.Student)
                .ThenInclude(s => s.User)
            .Where(se => se.SubjectId == subjectId && se.Status == ElectiveStatus.Enrolled)
            .Select(se => se.Student)
            .ToListAsync();

        return regularStudents.Union(electiveStudents).Distinct();
    }

    #endregion

    #region Grade-related methods

    public async Task<IEnumerable<Subject>> GetByGradeIdAsync(Guid gradeId)
    {
        return await _context.Subjects
            .Include(s => s.Teachers)
            .Include(s => s.Grades)
            .Where(s => s.Grades.Any(g => g.Id == gradeId))
            .ToListAsync();
    }

    #endregion

    #region Elective enrollment methods

    public async Task<bool> HasEnrolledStudentsAsync(Guid subjectId)
    {
        return await _context.StudentElectives
            .AnyAsync(se => se.SubjectId == subjectId && se.Status == ElectiveStatus.Enrolled);
    }

    public async Task<bool> IsStudentEnrolledAsync(Guid subjectId, Guid studentId)
    {
        return await _context.StudentElectives
            .AnyAsync(se => se.SubjectId == subjectId &&
                     se.StudentId == studentId &&
                     se.Status == ElectiveStatus.Enrolled);
    }

    public async Task<int> GetEnrollmentCountAsync(Guid subjectId)
    {
        return await _context.StudentElectives
            .CountAsync(se => se.SubjectId == subjectId &&
                       se.Status == ElectiveStatus.Enrolled);
    }

    public async Task<StudentElective> AddStudentElectiveAsync(StudentElective enrollment)
    {
        await _context.StudentElectives.AddAsync(enrollment);
        return enrollment;
    }

    public async Task UpdateStudentElectiveAsync(StudentElective enrollment)
    {
        _context.StudentElectives.Update(enrollment);
    }

    public async Task<StudentElective?> GetStudentElectiveAsync(Guid subjectId, Guid studentId)
    {
        return await _context.StudentElectives
            .FirstOrDefaultAsync(se => se.SubjectId == subjectId &&
                               se.StudentId == studentId);
    }

    public async Task DeleteStudentElectiveAsync(Guid enrollmentId)
    {
        var enrollment = await _context.StudentElectives.FindAsync(enrollmentId);
        if (enrollment != null)
        {
            _context.StudentElectives.Remove(enrollment);
        }
    }

    public async Task<IEnumerable<StudentElective>> GetStudentElectivesAsync(Guid subjectId)
    {
        return await _context.StudentElectives
            .Where(se => se.SubjectId == subjectId)
            .Include(se => se.Student)
            .ToListAsync();
    }

    public async Task<IEnumerable<StudentElective>> GetStudentEnrollmentsAsync(Guid studentId)
    {
        return await _context.StudentElectives
            .Where(se => se.StudentId == studentId)
            .Include(se => se.Subject)
            .ToListAsync();
    }

    public async Task<bool> UpdateStudentElectiveStatusAsync(Guid subjectId, Guid studentId, ElectiveStatus status)
    {
        var enrollment = await GetStudentElectiveAsync(subjectId, studentId);
        if (enrollment == null)
            return false;

        enrollment.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Bulk operations

    public async Task<IEnumerable<Subject>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _context.Subjects
            .Include(s => s.Department)
            .Include(s => s.Teachers)
            .Include(s => s.Grades)
            .Where(s => ids.Contains(s.Id))
            .ToListAsync();
    }

    public async Task UpdateTeachersAsync(Guid subjectId, IEnumerable<Guid> teacherIds)
    {
        var subject = await _context.Subjects
            .Include(s => s.Teachers)
            .FirstOrDefaultAsync(s => s.Id == subjectId);

        if (subject == null)
            return;

        subject.Teachers.Clear();
        var teachers = await _context.Teachers
            .Where(t => teacherIds.Contains(t.Id))
            .ToListAsync();

        foreach (var teacher in teachers)
        {
            subject.Teachers.Add(teacher);
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateGradesAsync(Guid subjectId, IEnumerable<Guid> gradeIds)
    {
        var subject = await _context.Subjects
            .Include(s => s.Grades)
            .FirstOrDefaultAsync(s => s.Id == subjectId);

        if (subject == null)
            return;

        subject.Grades.Clear();
        var grades = await _context.Grades
            .Where(g => gradeIds.Contains(g.Id))
            .ToListAsync();

        foreach (var grade in grades)
        {
            subject.Grades.Add(grade);
        }

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Combined operations

    public async Task<ElectiveEnrollmentValidation> ValidateElectiveEnrollmentAsync(Guid subjectId, Guid studentId)
    {
        var result = new ElectiveEnrollmentValidation();

        var subject = await _context.Subjects
            .AsNoTracking()
            .Select(s => new
            {
                s.Id,
                s.IsElective,
                s.MaxStudents
            })
            .FirstOrDefaultAsync(s => s.Id == subjectId);

        if (subject == null)
            return result;

        result.IsElective = subject.IsElective;
        result.MaxEnrollment = subject.MaxStudents;

        var studentExists = await _context.Students
            .AnyAsync(s => s.Id == studentId);

        result.StudentNotFound = !studentExists;

        if (studentExists)
        {
            result.AlreadyEnrolled = await _context.Set<StudentElective>()
                .AnyAsync(se =>
                    se.SubjectId == subjectId &&
                    se.StudentId == studentId &&
                    se.Status == ElectiveStatus.Enrolled);
        }

        if (subject.IsElective && subject.MaxStudents.HasValue)
        {
            result.CurrentEnrollment = await _context.Set<StudentElective>()
                .CountAsync(se =>
                    se.SubjectId == subjectId &&
                    se.Status == ElectiveStatus.Enrolled);

            result.MaxCapacityReached = result.CurrentEnrollment >= subject.MaxStudents.Value;
        }

        return result;
    }

    public async Task<ElectiveEnrollmentsData> GetElectiveEnrollmentsWithDetailsAsync(Guid subjectId)
    {
        var result = new ElectiveEnrollmentsData();

        var subject = await _context.Subjects
            .AsNoTracking()
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.IsElective
            })
            .FirstOrDefaultAsync(s => s.Id == subjectId);

        if (subject == null)
        {
            result.SubjectNotFound = true;
            return result;
        }

        result.IsElective = subject.IsElective;
        result.SubjectName = subject.Name;

        if (!subject.IsElective)
            return result;

        var enrollments = await _context.Set<StudentElective>()
            .AsNoTracking()
            .Include(se => se.Student)
                .ThenInclude(s => s.User)
            .Where(se => se.SubjectId == subjectId)
            .ToListAsync();

        result.Enrollments = enrollments;
        return result;
    }

    public async Task<StudentElectivesData> GetStudentElectivesWithDetailsAsync(Guid studentId)
    {
        var result = new StudentElectivesData();

        var student = await _context.Students
            .AsNoTracking()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student == null)
        {
            result.StudentNotFound = true;
            return result;
        }

        result.StudentName = $"{student.User?.FirstName} {student.User?.LastName}";

        var enrollments = await _context.Set<StudentElective>()
            .AsNoTracking()
            .Where(se =>
                se.StudentId == studentId &&
                se.Status == ElectiveStatus.Enrolled)
            .Select(se => se.SubjectId)
            .ToListAsync();

        if (!enrollments.Any())
            return result;

        var subjects = await _context.Subjects
            .AsNoTracking()
            .Include(s => s.PrimaryTeacher)
                .ThenInclude(t => t.User)
            .Include(s => s.Department)
            .Where(s => enrollments.Contains(s.Id))
            .ToListAsync();

        result.Subjects = subjects;
        return result;
    }

    public async Task<SubjectStudentsData> GetStudentsBySubjectWithTeacherValidationAsync(Guid subjectId, Guid teacherId)
    {
        var result = new SubjectStudentsData();

        var subject = await _context.Subjects
            .AsNoTracking()
            .Select(s => new
            {
                s.Id,
                s.Name,
                TeachesSubject = s.Teachers.Any(t => t.Id == teacherId) || s.PrimaryTeacherId == teacherId
            })
            .FirstOrDefaultAsync(s => s.Id == subjectId);

        if (subject == null)
            return result;

        result.SubjectName = subject.Name;
        result.TeacherTeachesSubject = subject.TeachesSubject;

        if (!subject.TeachesSubject)
            return result;

        result.Students = (await GetStudentsBySubjectIdAsync(subjectId)).ToList();
        return result;
    }

    public async Task<TeacherSubjectsData> GetAllSubjectsByTeacherAsync(Guid teacherId)
    {
        var result = new TeacherSubjectsData();

        var teacher = await _context.Teachers
            .AsNoTracking()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == teacherId);

        if (teacher == null)
        {
            result.TeacherNotFound = true;
            return result;
        }

        result.TeacherName = $"{teacher.User?.FirstName} {teacher.User?.LastName}";

        var directSubjects = await _context.Subjects
            .AsNoTracking()
            .Include(s => s.Department)
            .Include(s => s.PrimaryTeacher)
                .ThenInclude(t => t.User)
            .Where(s => s.Teachers.Any(t => t.Id == teacherId))
            .ToListAsync();

        var primarySubjects = await _context.Subjects
            .AsNoTracking()
            .Include(s => s.Department)
            .Include(s => s.PrimaryTeacher)
                .ThenInclude(t => t.User)
            .Where(s => s.PrimaryTeacherId == teacherId)
            .ToListAsync();

        var subjectIdsFromMarks = await _context.Marks
            .AsNoTracking()
            .Where(m => m.TeacherId == teacherId)
            .Select(m => m.SubjectId)
            .Distinct()
            .ToListAsync();

        var markedSubjects = await _context.Subjects
            .AsNoTracking()
            .Include(s => s.Department)
            .Include(s => s.PrimaryTeacher)
                .ThenInclude(t => t.User)
            .Where(s => subjectIdsFromMarks.Contains(s.Id))
            .ToListAsync();

        var subjectIds = new HashSet<Guid>();
        var allSubjects = new List<Subject>();

        foreach (var s in directSubjects.Concat(primarySubjects).Concat(markedSubjects))
        {
            if (subjectIds.Add(s.Id))
            {
                allSubjects.Add(s);
            }
        }

        result.Subjects = allSubjects;
        return result;
    }

    public async Task<SubjectStudentsByGradeData> GetStudentsBySubjectAndGradeWithTeacherValidationAsync(Guid subjectId, Guid teacherId)
    {
        var result = new SubjectStudentsByGradeData();

        var subject = await _context.Subjects
            .AsNoTracking()
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.IsElective,
                TeachesSubject = s.Teachers.Any(t => t.Id == teacherId) || s.PrimaryTeacherId == teacherId
            })
            .FirstOrDefaultAsync(s => s.Id == subjectId);

        if (subject == null)
        {
            result.SubjectNotFound = true;
            return result;
        }

        result.SubjectName = subject.Name;
        result.IsElective = subject.IsElective;
        result.TeacherTeachesSubject = subject.TeachesSubject;

        if (!subject.TeachesSubject)
            return result;

        var studentsByGrade = new Dictionary<Guid, StudentGradeGroup>();

        if (subject.IsElective)
        {
            var enrollments = await _context.Set<StudentElective>()
                .AsNoTracking()
                .Where(se =>
                    se.SubjectId == subjectId &&
                    se.Status == ElectiveStatus.Enrolled)
                .Include(se => se.Student)
                    .ThenInclude(s => s.User)
                .Include(se => se.Student)
                    .ThenInclude(s => s.Grade)
                .ToListAsync();

            foreach (var enrollment in enrollments)
            {
                var student = enrollment.Student;
                if (student?.Grade == null) continue;

                var gradeId = student.Grade.Id;
                var gradeName = student.Grade.Name;

                if (!studentsByGrade.TryGetValue(gradeId, out var gradeGroup))
                {
                    gradeGroup = new StudentGradeGroup
                    {
                        GradeId = gradeId,
                        GradeName = gradeName,
                        Students = new List<Student>()
                    };
                    studentsByGrade[gradeId] = gradeGroup;
                }

                gradeGroup.Students.Add(student);
            }
        }
        else
        {
            var subjectWithGrades = await _context.Subjects
                .AsNoTracking()
                .Include(s => s.Grades)
                    .ThenInclude(g => g.Students)
                        .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == subjectId);

            if (subjectWithGrades != null)
            {
                foreach (var grade in subjectWithGrades.Grades)
                {
                    var gradeGroup = new StudentGradeGroup
                    {
                        GradeId = grade.Id,
                        GradeName = grade.Name,
                        Students = grade.Students.ToList()
                    };

                    studentsByGrade[grade.Id] = gradeGroup;
                }
            }
        }

        result.StudentsByGrade = studentsByGrade.Values.ToList();
        return result;
    }

    #endregion
}