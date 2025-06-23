using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Api.Dto.Teacher;
using UniTrackRemaster.Api.Dto.Teacher.Analytics;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Repositories;

public class TeacherRepository(UniTrackDbContext context) : Repository<Teacher>(context), ITeacherRepository
{
    public async Task<Teacher?> GetByIdAsync(Guid id)
    {
        return await _context.Teachers
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Teacher>> GetAllAsync()
    {
        return await _context.Teachers
            .Include(t => t.User)
            .ToListAsync();
    }

    public async Task<Teacher> CreateAsync(Teacher teacher)
    {
        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();
        return teacher;
    }

    public async Task<(IEnumerable<Teacher> Teachers, int TotalCount)> SearchTeachersAsync(
        TeacherSearchParams searchParams)
    {
        var query = _context.Teachers
            .Include(t => t.User)
            .Include(t => t.Institution)
            .Include(t => t.ClassGrade)
            .AsQueryable();

        // Apply search query filter
        if (!string.IsNullOrWhiteSpace(searchParams.Query))
        {
            var searchTerm = searchParams.Query.ToLower().Trim();
            query = query.Where(t =>
                (t.User.FirstName != null && t.User.FirstName.ToLower().Contains(searchTerm)) ||
                (t.User.LastName != null && t.User.LastName.ToLower().Contains(searchTerm)) ||
                (t.User.Email != null && t.User.Email.ToLower().Contains(searchTerm)) ||
                (t.Title != null && t.Title.ToLower().Contains(searchTerm)) ||
                (t.User.FirstName + " " + t.User.LastName).ToLower().Contains(searchTerm)
            );
        }

        // Apply institution filter
        if (searchParams.InstitutionId.HasValue)
            query = query.Where(t => t.InstitutionId == searchParams.InstitutionId.Value);

        // Apply department filter (through subjects relationship)
        if (searchParams.DepartmentId.HasValue)
            query = query.Where(t => t.Subjects.Any(s => s.DepartmentId == searchParams.DepartmentId.Value));

        // Apply class grade filter (homeroom teacher)
        if (searchParams.ClassGradeId.HasValue)
            query = query.Where(t => t.ClassGradeId == searchParams.ClassGradeId.Value);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(searchParams.SortBy))
            query = searchParams.SortBy.ToLower() switch
            {
                "firstname" => searchParams.Ascending
                    ? query.OrderBy(t => t.User.FirstName)
                    : query.OrderByDescending(t => t.User.FirstName),
                "lastname" => searchParams.Ascending
                    ? query.OrderBy(t => t.User.LastName)
                    : query.OrderByDescending(t => t.User.LastName),
                "email" => searchParams.Ascending
                    ? query.OrderBy(t => t.User.Email)
                    : query.OrderByDescending(t => t.User.Email),
                "title" => searchParams.Ascending
                    ? query.OrderBy(t => t.Title)
                    : query.OrderByDescending(t => t.Title),
                "institution" => searchParams.Ascending
                    ? query.OrderBy(t => t.Institution.Name)
                    : query.OrderByDescending(t => t.Institution.Name),
                "createdat" => searchParams.Ascending
                    ? query.OrderBy(t => t.CreatedAt)
                    : query.OrderByDescending(t => t.CreatedAt),
                _ => searchParams.Ascending
                    ? query.OrderBy(t => t.User.LastName).ThenBy(t => t.User.FirstName)
                    : query.OrderByDescending(t => t.User.LastName).ThenByDescending(t => t.User.FirstName)
            };
        else
            // Default sorting by last name, first name
            query = searchParams.Ascending
                ? query.OrderBy(t => t.User.LastName).ThenBy(t => t.User.FirstName)
                : query.OrderByDescending(t => t.User.LastName).ThenByDescending(t => t.User.FirstName);

        // Apply pagination
        var teachers = await query
            .Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
            .Take(searchParams.PageSize)
            .AsNoTracking()
            .ToListAsync();

        return (teachers, totalCount);
    }

    public async Task UpdateAsync(Teacher teacher)
    {
        _context.Teachers.Update(teacher);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var teacher = await GetByIdAsync(id);
        if (teacher != null)
        {
            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Teacher?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Teachers
            .Include(t => t.User)
            .Include(t => t.Institution)
            .Include(t => t.ClassGrade)
            .Include(t => t.HomeRoomAssignments)
            .FirstOrDefaultAsync(t => t.UserId == userId);
    }

    public async Task<IEnumerable<Teacher>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _context.Teachers
            .Include(t => t.User)
            .Include(t => t.ClassGrade)
            .Where(t => ids.Contains(t.Id))
            .ToListAsync();
    }

    public async Task<Teacher?> GetByIdWithUserAsync(Guid id)
    {
        return await _context.Teachers
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Teacher>> GetAllWithUsersAsync()
    {
        return await _context.Teachers
            .Include(t => t.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<Teacher>> GetByInstitutionIdWithUsersAsync(Guid institutionId)
    {
        return await _context.Teachers
            .Include(t => t.User)
            .Where(t => t.InstitutionId == institutionId)
            .ToListAsync();
    }

    public async Task<Teacher?> GetByUserIdWithUserAsync(Guid userId)
    {
        return await _context.Teachers
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.UserId == userId);
    }

    public async Task<IEnumerable<Teacher>> GetByInstitutionIdAsync(Guid institutionId)
    {
        return await _context.Teachers
            .Where(t => t.InstitutionId == institutionId)
            .ToListAsync();
    }

    // OPTIMIZED: Gets comprehensive teacher dashboard data - balanced approach
    public async Task<(
        int SubjectsCount,
        int TotalStudents,
        int TotalMarks,
        int TotalAbsences,
        List<Subject> Subjects,
        List<StudentGradeGroup> StudentsByGrade,
        List<Mark> RecentMarks,
        List<Absence> RecentAbsences,
        Dictionary<string, decimal> AverageMarksBySubject
        )> GetTeacherDashboardDataAsync(Guid teacherId, Guid? semesterId)
    {
        // 1. Check if teacher exists (lightweight)
        var teacherExists = await _context.Teachers
            .AsNoTracking()
            .AnyAsync(t => t.Id == teacherId);

        if (!teacherExists)
            return (0, 0, 0, 0, new List<Subject>(), new List<StudentGradeGroup>(), 
                    new List<Mark>(), new List<Absence>(), new Dictionary<string, decimal>());

        // 2. Get subjects efficiently (keep entity loading for DTOs)
        var subjects = await _context.Subjects
            .AsNoTracking()
            .Where(s => s.Teachers.Any(t => t.Id == teacherId) || s.PrimaryTeacherId == teacherId)
            .ToListAsync();

        var subjectsCount = subjects.Count;

        // 3. Get teacher with grade relationships (optimized loading)
        var teacherWithGrades = await _context.Teachers
            .AsNoTracking()
            .Where(t => t.Id == teacherId)
            .Include(t => t.Grades)
            .ThenInclude(g => g.Students)
            .ThenInclude(s => s.User)
            .Include(t => t.ClassGrade)
            .ThenInclude(g => g.Students)
            .ThenInclude(s => s.User)
            .FirstOrDefaultAsync();

        // 4. Process students by grade (use loaded entities)
        var studentsByGrade = new List<StudentGradeGroup>();
        var allStudentIds = new HashSet<Guid>();

        if (teacherWithGrades != null)
        {
            // Add students from assigned grades
            if (teacherWithGrades.Grades != null)
            {
                foreach (var grade in teacherWithGrades.Grades)
                {
                    var gradeGroup = new StudentGradeGroup
                    {
                        GradeId = grade.Id,
                        GradeName = grade.Name,
                        Students = grade.Students?.ToList() ?? new List<Student>()
                    };
                    
                    studentsByGrade.Add(gradeGroup);
                    
                    // Track student IDs
                    foreach (var student in gradeGroup.Students)
                        allStudentIds.Add(student.Id);
                }
            }

            // Add homeroom students if not already included
            if (teacherWithGrades.ClassGrade?.Students != null)
            {
                var homeroomGradeId = teacherWithGrades.ClassGrade.Id;
                var existingGrade = studentsByGrade.FirstOrDefault(g => g.GradeId == homeroomGradeId);
                
                if (existingGrade == null)
                {
                    var homeroomGroup = new StudentGradeGroup
                    {
                        GradeId = homeroomGradeId,
                        GradeName = teacherWithGrades.ClassGrade.Name,
                        Students = teacherWithGrades.ClassGrade.Students.ToList()
                    };
                    
                    studentsByGrade.Add(homeroomGroup);
                    
                    foreach (var student in homeroomGroup.Students)
                        allStudentIds.Add(student.Id);
                }
                else
                {
                    // Add homeroom students not already in the grade
                    foreach (var student in teacherWithGrades.ClassGrade.Students)
                    {
                        if (!existingGrade.Students.Any(s => s.Id == student.Id))
                        {
                            existingGrade.Students.Add(student);
                            allStudentIds.Add(student.Id);
                        }
                    }
                }
            }
        }

        var totalStudents = allStudentIds.Count;

// 5. Get counts efficiently with semester filtering
        var marksQuery = _context.Marks.AsNoTracking().Where(m => m.TeacherId == teacherId);
        var absencesQuery = _context.Absences.AsNoTracking().Where(a => a.TeacherId == teacherId);

        if (semesterId.HasValue)
        {
            marksQuery = marksQuery.Where(m => m.SemesterId == semesterId.Value);
            absencesQuery = absencesQuery.Where(a => a.SemesterId == semesterId.Value);
        }

// Execute queries sequentially (EF Core DbContext is not thread-safe)
        var totalMarks = await marksQuery.CountAsync();
        var totalAbsences = await absencesQuery.CountAsync();

// 6. Get recent items (only what we need)
        var recentMarks = await marksQuery
            .OrderByDescending(m => m.CreatedAt)
            .Take(10)
            .Include(m => m.Subject)
            .Include(m => m.Student)
            .ThenInclude(s => s.User)
            .ToListAsync();

        var recentAbsences = await absencesQuery
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .Include(a => a.Student)
            .ThenInclude(s => s.User)
            .Include(a => a.Subject)
            .ToListAsync();

// 7. Calculate averages with database aggregation
        var averageMarksBySubject = await marksQuery
            .GroupBy(m => m.Subject.Name)
            .Select(g => new { SubjectName = g.Key, Average = g.Average(m => m.Value) })
            .ToDictionaryAsync(x => x.SubjectName ?? "Unknown", x => x.Average);

        return (
            subjectsCount,
            totalStudents,
            totalMarks,
            totalAbsences,
            subjects,
            studentsByGrade,
            recentMarks,
            recentAbsences,
            averageMarksBySubject
        );
    }

    public async Task<IEnumerable<StudentGradeGroup>> GetStudentsBySubjectAndGradeAsync(Guid teacherId, Guid subjectId)
    {
        // Verify teacher teaches this subject
        var teachesSubject = await _context.Subjects
            .AsNoTracking()
            .AnyAsync(s => s.Id == subjectId &&
                           (s.Teachers.Any(t => t.Id == teacherId) || s.PrimaryTeacherId == teacherId));

        if (!teachesSubject)
            return Enumerable.Empty<StudentGradeGroup>();

        var result = new List<StudentGradeGroup>();

        // Get subject with relations
        var subject = await _context.Subjects
            .AsNoTracking()
            .Include(s => s.Grades)
            .ThenInclude(g => g.Students)
            .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == subjectId);

        if (subject == null)
            return Enumerable.Empty<StudentGradeGroup>();

        if (subject.IsElective)
        {
            // For elective subjects, group students through enrollments
            var enrollments = await _context.Set<StudentElective>()
                .AsNoTracking()
                .Where(se => se.SubjectId == subjectId &&
                             se.Status == ElectiveStatus.Enrolled)
                .Include(se => se.Student)
                .ThenInclude(s => s.Grade)
                .ThenInclude(g => g.Students)
                .ThenInclude(s => s.User)
                .ToListAsync();

            var gradeGroups = new Dictionary<Guid, StudentGradeGroup>();

            foreach (var enrollment in enrollments)
            {
                var student = enrollment.Student;
                if (student?.Grade == null) continue;

                var gradeId = student.Grade.Id;

                if (!gradeGroups.TryGetValue(gradeId, out var group))
                {
                    group = new StudentGradeGroup
                    {
                        GradeId = gradeId,
                        GradeName = student.Grade.Name,
                        Students = new List<Student>()
                    };
                    gradeGroups[gradeId] = group;
                }

                // Add student if not already in the list
                if (!group.Students.Any(s => s.Id == student.Id)) group.Students.Add(student);
            }

            result.AddRange(gradeGroups.Values);
        }
        else
        {
            // For regular subjects, use the direct subject-grade relationship
            foreach (var grade in subject.Grades)
            {
                var gradeGroup = new StudentGradeGroup
                {
                    GradeId = grade.Id,
                    GradeName = grade.Name,
                    Students = grade.Students.ToList()
                };

                result.Add(gradeGroup);
            }
        }

        return result;
    }

    // OPTIMIZED: Gets student absence data for at-risk student analysis with semester support
    public async Task<IEnumerable<AtRiskStudentDto>> GetStudentAbsenceDataAsync(
        Guid teacherId,
        Guid? gradeId = null,
        Guid? subjectId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        Guid? semesterId = null)
    {
        // Get absences with proper entity loading
        var absences = await GetTeacherAttendanceDataAsync(
            teacherId, fromDate, toDate, gradeId, subjectId, semesterId);

        // Group by student and build DTOs
        var studentGroups = absences
            .GroupBy(a => new
            {
                a.StudentId,
                FirstName = a.Student?.User?.FirstName ?? "Unknown",
                LastName = a.Student?.User?.LastName ?? "Unknown", 
                a.Student.GradeId,
                GradeName = a.Student?.Grade?.Name ?? "Unknown"
            })
            .ToList();

        var result = new List<AtRiskStudentDto>();

        foreach (var group in studentGroups)
        {
            var recentPatterns = group
                .OrderByDescending(a => a.Date)
                .Take(5)
                .Select(a => a.Status.ToString().ToLower())
                .ToList();

            result.Add(new AtRiskStudentDto
            {
                Id = group.Key.StudentId,
                FirstName = group.Key.FirstName,
                LastName = group.Key.LastName,
                GradeId = group.Key.GradeId,
                GradeName = group.Key.GradeName,
                TotalAbsences = group.Count(),
                AbsenceRate = 0, // Calculated in service
                RiskLevel = "none", // Determined in service
                RecentPattern = recentPatterns
            });
        }

        return result;
    }

    // OPTIMIZED: Gets comprehensive attendance statistics for a teacher with semester support
    public async Task<AttendanceStatisticsDto> GetAttendanceStatisticsAsync(
        Guid teacherId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        Guid? gradeId = null,
        Guid? subjectId = null,
        Guid? semesterId = null)
    {
        var fromDateUtc = fromDate?.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc)
            : fromDate?.ToUniversalTime();

        var toDateUtc = toDate?.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc)
            : toDate?.ToUniversalTime();

        var baseQuery = _context.Absences.AsNoTracking()
            .Where(a => a.TeacherId == teacherId);

        // Apply filters
        if (semesterId.HasValue) 
            baseQuery = baseQuery.Where(a => a.SemesterId == semesterId.Value);
        if (subjectId.HasValue) 
            baseQuery = baseQuery.Where(a => a.SubjectId == subjectId.Value);
        if (gradeId.HasValue) 
            baseQuery = baseQuery.Where(a => a.Student.GradeId == gradeId.Value);
        if (fromDateUtc.HasValue) 
            baseQuery = baseQuery.Where(a => a.Date >= fromDateUtc.Value);
        if (toDateUtc.HasValue) 
            baseQuery = baseQuery.Where(a => a.Date <= toDateUtc.Value);

        // Execute parallel aggregation queries
        var totalAbsencesTask = baseQuery.CountAsync();
        var totalStudentsTask = baseQuery.Select(a => a.StudentId).Distinct().CountAsync();
        
        var absencesByStatusTask = baseQuery
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
            
        var absencesBySubjectTask = baseQuery
            .GroupBy(a => a.Subject.Name)
            .Select(g => new { Subject = g.Key ?? "Unknown", Count = g.Count() })
            .ToDictionaryAsync(x => x.Subject, x => x.Count);
            
        var absencesByGradeTask = baseQuery
            .GroupBy(a => a.Student.Grade.Name)
            .Select(g => new { Grade = g.Key ?? "Unknown", Count = g.Count() })
            .ToDictionaryAsync(x => x.Grade, x => x.Count);

        // Get minimal data for trends
        var trendDataTask = baseQuery
            .Select(a => a.Date)
            .ToListAsync();

        // Wait for all operations
        await Task.WhenAll(totalAbsencesTask, totalStudentsTask, absencesByStatusTask, 
                          absencesBySubjectTask, absencesByGradeTask, trendDataTask);

        var totalAbsences = await totalAbsencesTask;
        var totalStudents = await totalStudentsTask;

        if (totalAbsences == 0)
        {
            return new AttendanceStatisticsDto
            {
                TotalStudents = 0,
                TotalAbsences = 0,
                AttendanceRate = 100,
                AbsencesByStatus = new Dictionary<string, int>(),
                AbsencesBySubject = new Dictionary<string, int>(),
                AbsencesByGrade = new Dictionary<string, int>(),
                TrendByDay = new Dictionary<string, int>(),
                TrendByWeek = new Dictionary<string, int>(),
                TrendByMonth = new Dictionary<string, int>()
            };
        }

        // Calculate attendance rate
        var periodStart = fromDateUtc ?? DateTime.UtcNow.AddMonths(-1);
        var periodEnd = toDateUtc ?? DateTime.UtcNow;
        var totalClassDays = CountBusinessDays(periodStart, periodEnd);
        
        decimal totalPossibleAttendances = totalStudents * totalClassDays;
        var attendanceRate = totalPossibleAttendances > 0
            ? Math.Round((totalPossibleAttendances - totalAbsences) / totalPossibleAttendances * 100, 1)
            : 100;

        var trendData = await trendDataTask;

        return new AttendanceStatisticsDto
        {
            TotalStudents = totalStudents,
            TotalAbsences = totalAbsences,
            AttendanceRate = attendanceRate,
            AbsencesByStatus = await absencesByStatusTask,
            AbsencesBySubject = await absencesBySubjectTask,
            AbsencesByGrade = await absencesByGradeTask,
            TrendByDay = GetDailyTrendOptimized(trendData),
            TrendByWeek = GetWeeklyTrendOptimized(trendData),
            TrendByMonth = GetMonthlyTrendOptimized(trendData)
        };
    }

    // OPTIMIZED: Gets absence data for a teacher grouped by excused/unexcused status with semester support
    public async Task<IEnumerable<SubjectExcusedUnexcusedDto>> GetTeacherAbsencesBreakdownAsync(
        Guid teacherId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        Guid? gradeId = null,
        Guid? subjectId = null,
        Guid? semesterId = null)
    {
        // Get filtered absences using the existing optimized method
        var absences = await GetTeacherAttendanceDataAsync(
            teacherId,
            fromDate,
            toDate,
            gradeId,
            subjectId,
            semesterId);

        // Group by subject and calculate the breakdown
        return absences
            .GroupBy(a => new
            {
                a.SubjectId,
                SubjectName = a.Subject?.Name ?? "Unknown Subject"
            })
            .Select(g =>
            {
                var total = g.Count();
                var excusedCount = g.Count(a => a.IsExcused);
                var unexcusedCount = total - excusedCount;

                return new SubjectExcusedUnexcusedDto
                {
                    SubjectId = g.Key.SubjectId,
                    SubjectName = g.Key.SubjectName,
                    ExcusedCount = excusedCount,
                    UnexcusedCount = unexcusedCount,
                    Total = total,
                    ExcusedPercentage = total > 0 ? Math.Round((decimal)excusedCount / total * 100, 1) : 0,
                    UnexcusedPercentage = total > 0 ? Math.Round((decimal)unexcusedCount / total * 100, 1) : 0
                };
            })
            .OrderByDescending(s => s.Total)
            .ToList();
    }

    // LEGACY METHODS - keeping for backward compatibility
    public async Task<IEnumerable<AtRiskStudentDto>> GetStudentAbsenceDataAsync(
        Guid teacherId,
        Guid? gradeId = null,
        Guid? subjectId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        return await GetStudentAbsenceDataAsync(teacherId, gradeId, subjectId, fromDate, toDate, null);
    }

    public async Task<AttendanceStatisticsDto> GetAttendanceStatisticsAsync(
        Guid teacherId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        Guid? gradeId = null,
        Guid? subjectId = null)
    {
        return await GetAttendanceStatisticsAsync(teacherId, fromDate, toDate, gradeId, subjectId, null);
    }

    public async Task<IEnumerable<SubjectExcusedUnexcusedDto>> GetTeacherAbsencesBreakdownAsync(
        Guid teacherId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        Guid? gradeId = null,
        Guid? subjectId = null)
    {
        return await GetTeacherAbsencesBreakdownAsync(teacherId, fromDate, toDate, gradeId, subjectId, null);
    }

    // OPTIMIZED: Helper method to get attendance data efficiently
    public async Task<IEnumerable<Absence>> GetTeacherAttendanceDataAsync(
        Guid teacherId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        Guid? gradeId = null,
        Guid? subjectId = null,
        Guid? semesterId = null)
    {
        var fromDateUtc = fromDate?.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc)
            : fromDate?.ToUniversalTime();

        var toDateUtc = toDate?.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc)
            : toDate?.ToUniversalTime();

        var query = _context.Absences.AsNoTracking()
            .Where(a => a.TeacherId == teacherId);

        // Apply all filters
        if (semesterId.HasValue) 
            query = query.Where(a => a.SemesterId == semesterId.Value);
        if (subjectId.HasValue) 
            query = query.Where(a => a.SubjectId == subjectId.Value);
        if (gradeId.HasValue) 
            query = query.Where(a => a.Student.GradeId == gradeId.Value);
        if (fromDateUtc.HasValue) 
            query = query.Where(a => a.Date >= fromDateUtc.Value);
        if (toDateUtc.HasValue) 
            query = query.Where(a => a.Date <= toDateUtc.Value);

        // Load with necessary relationships
        return await query
            .Include(a => a.Student)
            .ThenInclude(s => s.User)
            .Include(a => a.Student)
            .ThenInclude(s => s.Grade)
            .Include(a => a.Subject)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    // Optimized trend calculation methods
    private Dictionary<string, int> GetDailyTrendOptimized(IEnumerable<DateTime> absenceDates)
    {
        var trendByDay = new Dictionary<string, int>();
        var now = DateTime.Now;
        var thirtyDaysAgo = now.AddDays(-30);

        var absencesByDay = absenceDates
            .Where(d => d.ToLocalTime().Date >= thirtyDaysAgo.Date && d.ToLocalTime().Date <= now.Date)
            .GroupBy(d => d.ToLocalTime().Date)
            .ToDictionary(g => g.Key, g => g.Count());

        for (var date = thirtyDaysAgo.Date; date <= now.Date; date = date.AddDays(1))
        {
            var dateStr = date.ToString("MM/dd");
            trendByDay[dateStr] = absencesByDay.GetValueOrDefault(date, 0);
        }

        return trendByDay;
    }

    private Dictionary<string, int> GetWeeklyTrendOptimized(IEnumerable<DateTime> absenceDates)
    {
        var trendByWeek = new Dictionary<string, int>();
        var now = DateTime.Now;
        var absencesByDate = absenceDates
            .Select(d => d.ToLocalTime().Date)
            .GroupBy(d => d)
            .ToDictionary(g => g.Key, g => g.Count());

        for (var i = 0; i < 10; i++)
        {
            var weekStart = now.AddDays(-7 * (10 - i)).Date;
            var weekEnd = weekStart.AddDays(6);
            var weekLabel = $"W{i + 1}";

            trendByWeek[weekLabel] = absencesByDate
                .Where(kvp => kvp.Key >= weekStart && kvp.Key <= weekEnd)
                .Sum(kvp => kvp.Value);
        }

        return trendByWeek;
    }

    private Dictionary<string, int> GetMonthlyTrendOptimized(IEnumerable<DateTime> absenceDates)
    {
        var trendByMonth = new Dictionary<string, int>();
        var now = DateTime.Now;
        var absencesByMonth = absenceDates
            .Select(d => d.ToLocalTime())
            .GroupBy(d => new { d.Year, d.Month })
            .ToDictionary(g => g.Key, g => g.Count());

        for (var i = 5; i >= 0; i--)
        {
            var monthDate = now.AddMonths(-i);
            var monthLabel = monthDate.ToString("MMM");
            var monthKey = new { monthDate.Year, monthDate.Month };

            trendByMonth[monthLabel] = absencesByMonth.GetValueOrDefault(monthKey, 0);
        }

        return trendByMonth;
    }

    private int CountBusinessDays(DateTime startDate, DateTime endDate)
    {
        var days = 0;
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                days++;

        return Math.Max(1, days); // Ensure at least 1 day to avoid division by zero
    }

    public async Task<IEnumerable<Teacher>> GetPendingByUserIdAsync(Guid userId)
    {
        return await _context.Teachers
            .Include(t => t.User)
            .Include(t => t.Institution)
            .Include(t => t.ClassGrade)
            .Where(t => t.UserId == userId && t.Status == ProfileStatus.Pending)
            .ToListAsync();
    }

    public async Task<IEnumerable<Teacher>> GetByInstitutionAsync(Guid institutionId, ProfileStatus? status = null)
    {
        var query = _context.Teachers
            .Include(t => t.User)
            .Include(t => t.ClassGrade)
            .Where(t => t.InstitutionId == institutionId);

        if (status.HasValue) query = query.Where(t => t.Status == status.Value);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Teacher>> GetByStatusAsync(ProfileStatus status)
    {
        return await _context.Teachers
            .Include(t => t.User)
            .Include(t => t.Institution)
            .Where(t => t.Status == status)
            .ToListAsync();
    }

    public async Task AssignGradesAsync(Guid teacherId, IEnumerable<Guid> gradeIds)
    {
        var teacher = await _context.Teachers
            .Include(t => t.Grades)
            .FirstOrDefaultAsync(t => t.Id == teacherId);

        if (teacher == null) return;

        var grades = await _context.Grades
            .Where(g => gradeIds.Contains(g.Id))
            .ToListAsync();

        foreach (var grade in grades)
        {
            if (teacher.Grades == null)
                teacher.Grades = new List<Grade>();

            if (!teacher.Grades.Any(g => g.Id == grade.Id)) teacher.Grades.Add(grade);
        }

        await _context.SaveChangesAsync();
    }

    public async Task UnassignGradesAsync(Guid teacherId, IEnumerable<Guid> gradeIds)
    {
        var teacher = await _context.Teachers
            .Include(t => t.Grades)
            .FirstOrDefaultAsync(t => t.Id == teacherId);

        if (teacher?.Grades == null) return;

        var gradesToRemove = teacher.Grades
            .Where(g => gradeIds.Contains(g.Id))
            .ToList();

        foreach (var grade in gradesToRemove) teacher.Grades.Remove(grade);

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Grade>> GetAssignedGradesAsync(Guid teacherId)
    {
        var teacher = await _context.Teachers
            .AsNoTracking()
            .Include(t => t.Grades)
            .ThenInclude(g => g.Institution)
            .Include(t => t.Grades)
            .ThenInclude(g => g.AcademicYear)
            .Include(t => t.Grades)
            .ThenInclude(g => g.Students)
            .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(t => t.Id == teacherId);

        return teacher?.Grades ?? new List<Grade>();
    }

    public async Task UpdateGradeAssignmentsAsync(Guid teacherId, IEnumerable<Guid> gradeIds)
    {
        var teacher = await _context.Teachers
            .Include(t => t.Grades)
            .FirstOrDefaultAsync(t => t.Id == teacherId);

        if (teacher == null) return;

        // Clear existing assignments
        if (teacher.Grades != null)
            teacher.Grades.Clear();
        else
            teacher.Grades = new List<Grade>();

        // Add new assignments
        if (gradeIds.Any())
        {
            var grades = await _context.Grades
                .Where(g => gradeIds.Contains(g.Id))
                .ToListAsync();

            foreach (var grade in grades) teacher.Grades.Add(grade);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Teacher?> GetTeacherWithGradesAsync(Guid teacherId)
    {
        return await _context.Teachers
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Institution)
            .Include(t => t.ClassGrade)
            .Include(t => t.Grades)
            .ThenInclude(g => g.Institution)
            .Include(t => t.Grades)
            .ThenInclude(g => g.AcademicYear)
            .Include(t => t.Subjects)
            .FirstOrDefaultAsync(t => t.Id == teacherId);
    }
}