using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Absence;
using UniTrackRemaster.Api.Dto.Mark;
using UniTrackRemaster.Api.Dto.Student;
using UniTrackRemaster.Api.Dto.Subject;
using UniTrackRemaster.Api.Dto.Teacher;
using UniTrackRemaster.Api.Dto.Teacher.Analytics;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Services.User.Teachers;

public class TeacherService(
    IUnitOfWork unitOfWork,
    ILogger<TeacherService> logger,
    UserManager<ApplicationUser> userManager,
    IGradingSystemService gradingSystemService) : ITeacherService
{
    public async Task<TeacherResponseDto> GetByIdAsync(Guid id)
    {
        var teacher = await unitOfWork.Teachers.GetByIdWithUserAsync(id) ?? throw new NotFoundException("Teacher not found");
        return TeacherResponseDto.FromEntity(teacher, teacher.User);
    }

    public async Task<IEnumerable<TeacherResponseDto>> GetAllAsync()
    {
        var teachers = await unitOfWork.Teachers.GetAllWithUsersAsync();
        return teachers.Select(t => TeacherResponseDto.FromEntity(t, t.User));
    }

    public async Task<IEnumerable<TeacherResponseDto>> GetByInstitutionIdAsync(Guid id)
    {
        var teachers = await unitOfWork.Teachers.GetByInstitutionIdWithUsersAsync(id);
        return teachers.Select(t => TeacherResponseDto.FromEntity(t, t.User ?? throw new InvalidOperationException()));
    }

    public async Task<TeacherResponseDto> CreateAsync(CreateTeacherDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ValidationException("Teacher email is required");
        if (string.IsNullOrWhiteSpace(dto.FirstName))
            throw new ValidationException("Teacher first name is required");
        if (string.IsNullOrWhiteSpace(dto.LastName))
            throw new ValidationException("Teacher last name is required");

        await unitOfWork.BeginTransactionAsync();
        try
        {
            // Look up existing user
            var user = await userManager.FindByEmailAsync(dto.Email) ??
                throw new NotFoundException($"Application user with email {dto.Email} not found. Please create a user account first.");

            // Check if user already has a teacher profile (any status)
            var existingTeacher = await unitOfWork.Teachers.GetByUserIdAsync(user.Id);
            if (existingTeacher != null)
                throw new InvalidOperationException($"User with email {dto.Email} already has a teacher profile.");

            // Verify institution exists
            var institution = await unitOfWork.Institutions.GetByIdAsync(dto.InstitutionId) ??
                throw new NotFoundException($"Institution with ID {dto.InstitutionId} not found.");

            // Verify class grade if specified
            if (dto.ClassGradeId.HasValue)
            {
                var grade = await unitOfWork.Grades.GetByIdAsync(dto.ClassGradeId.Value) ??
                    throw new NotFoundException($"Grade with ID {dto.ClassGradeId} not found.");

                if (grade.InstitutionId != dto.InstitutionId)
                    throw new ValidationException("The specified grade does not belong to the specified institution.");
            }

            // Create teacher entity with PENDING status
            var teacher = new Teacher
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Title = dto.Title,
                InstitutionId = dto.InstitutionId,
                ClassGradeId = dto.ClassGradeId,
                Status = ProfileStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await unitOfWork.Teachers.CreateAsync(teacher);
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();

            return TeacherResponseDto.FromEntity(teacher, user);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();

            if (ex is NotFoundException || ex is ValidationException || ex is InvalidOperationException)
                throw;

            logger.LogError(ex, "Error creating teacher invitation: {Message}", ex.Message);
            throw new ApplicationException("An error occurred while creating the teacher invitation.", ex);
        }
    }

    public async Task<TeacherSearchResponse> SearchTeachersAsync(TeacherSearchParams searchParams)
    {
        try
        {
            // Validate search parameters
            if (searchParams.PageNumber < 1)
                searchParams.PageNumber = 1;

            if (searchParams.PageSize < 1 || searchParams.PageSize > 100)
                searchParams.PageSize = 10;

            // Perform the search
            var (teachers, totalCount) = await unitOfWork.Teachers.SearchTeachersAsync(searchParams);

            // Convert to DTOs
            var teacherDtos = teachers.Select(t => TeacherResponseDto.FromEntity(t, t.User));

            // Calculate pagination info
            var totalPages = (int)Math.Ceiling((double)totalCount / searchParams.PageSize);
            var hasPreviousPage = searchParams.PageNumber > 1;
            var hasNextPage = searchParams.PageNumber < totalPages;

            return new TeacherSearchResponse
            {
                Teachers = teacherDtos,
                TotalCount = totalCount,
                PageNumber = searchParams.PageNumber,
                PageSize = searchParams.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = hasPreviousPage,
                HasNextPage = hasNextPage
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching teachers with parameters: {@SearchParams}", searchParams);
            throw new ApplicationException("An error occurred while searching teachers.", ex);
        }
    }

    public async Task<TeacherResponseDto?> GetByUserIdAsync(Guid userId)
    {
        try
        {
            var teacher = await unitOfWork.Teachers.GetByUserIdWithUserAsync(userId);
            if (teacher is null || teacher.User is null)
                return null;

            return TeacherResponseDto.FromEntity(teacher, teacher.User);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving teacher by user ID {UserId}: {Message}",
                userId, ex.Message);
            throw;
        }
    }

    public async Task<TeacherResponseDto> UpdateAsync(Guid id, UpdateTeacherDto dto)
    {
        var teacher = await unitOfWork.Teachers.GetByIdWithUserAsync(id) ?? throw new NotFoundException("Teacher not found");
        if (dto.Title != null) teacher.Title = dto.Title;
        if (dto.ClassGradeId.HasValue) teacher.ClassGradeId = dto.ClassGradeId;

        await unitOfWork.Teachers.UpdateAsync(teacher);
        return TeacherResponseDto.FromEntity(teacher, teacher.User);
    }

    public async Task DeleteAsync(Guid id)
    {
        var teacher = await unitOfWork.Teachers.GetByIdAsync(id) ?? throw new NotFoundException("Teacher not found");
        await unitOfWork.Teachers.DeleteAsync(id);
    }

    public async Task<IEnumerable<StudentsByGradeDto>> GetStudentsBySubjectAndGradeAsync(Guid teacherId, Guid subjectId)
    {
        try
        {
            // Verify teacher exists
            var teacher = await unitOfWork.Teachers.GetByIdAsync(teacherId) ?? throw new NotFoundException("Teacher not found");

            // Get student data grouped by grade in a single efficient query
            var studentGradeGroups = await unitOfWork.Teachers.GetStudentsBySubjectAndGradeAsync(teacherId, subjectId);

            return studentGradeGroups.Select(g => new StudentsByGradeDto(
                g.GradeId,
                g.GradeName,
                g.Students.Select(StudentResponseDto.FromEntity).ToList()
            )).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving students by subject and grade for teacher {TeacherId}: {Message}",
                teacherId, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// FIXED: Gets the teacher dashboard with proper error handling and entity validation
    /// </summary>
    public async Task<TeacherDashboardDto> GetTeacherDashboardAsync(Guid teacherId, Guid? semesterId = null)
    {
        try
        {
            // 1. Verify teacher exists and get basic info
            var teacher = await unitOfWork.Teachers.GetByIdWithUserAsync(teacherId);
            if (teacher == null)
                throw new NotFoundException("Teacher not found");

            // 2. Get current semester if none specified
            var currentSemester = semesterId.HasValue 
                ? await unitOfWork.Semesters.GetByIdAsync(semesterId.Value)
                : await GetCurrentSemesterAsync(teacher.InstitutionId);

            if (currentSemester == null)
                throw new NotFoundException("No active semester found");

            // 3. Get institution's grading system
            var gradingSystem = await GetInstitutionGradingSystemAsync(teacher.InstitutionId);

            // 4. Get dashboard data with proper semester filtering
            var dashboardData = await unitOfWork.Teachers.GetTeacherDashboardDataAsync(teacherId, currentSemester.Id);

            // 5. Validate that we have the expected data structure
            if (dashboardData.StudentsByGrade == null)
            {
                logger.LogWarning("No student grade groups returned for teacher {TeacherId}", teacherId);
                dashboardData.StudentsByGrade = new List<StudentGradeGroup>();
            }

            // 6. Calculate average marks with grading system integration
            var averageMarksBySubjectWithGrades = new Dictionary<string, decimal>();
            if (dashboardData.AverageMarksBySubject != null)
            {
                averageMarksBySubjectWithGrades = dashboardData.AverageMarksBySubject
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            // 7. Process recent marks with error handling
            var processedRecentMarks = new List<MarkResponseDto>();
            if (dashboardData.RecentMarks != null)
            {
                foreach (var mark in dashboardData.RecentMarks)
                {
                    try
                    {
                        var enhancedMark = EnhanceMarkWithGradingSystem(mark, gradingSystem);
                        processedRecentMarks.Add(enhancedMark);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to enhance mark {MarkId} with grading system", mark.Id);
                        // Add the mark without enhancement
                        processedRecentMarks.Add(MarkResponseDto.FromEntity(mark));
                    }
                }
            }

            // 8. Process recent absences with error handling
            var processedRecentAbsences = new List<AbsenceResponseDto>();
            if (dashboardData.RecentAbsences != null)
            {
                processedRecentAbsences = dashboardData.RecentAbsences
                    .Select(a =>
                    {
                        try
                        {
                            return AbsenceResponseDto.FromEntity(a);
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Failed to convert absence {AbsenceId} to DTO", a.Id);
                            return null;
                        }
                    })
                    .Where(dto => dto != null)
                    .ToList();
            }

            // 9. Process students by grade with validation
            var processedStudentsByGrade = new List<StudentsByGradeDto>();
            if (dashboardData.StudentsByGrade != null)
            {
                foreach (var gradeGroup in dashboardData.StudentsByGrade)
                {
                    try
                    {
                        var students = gradeGroup.Students?
                            .Where(s => s.User != null) // Ensure user is loaded
                            .Select(s =>
                            {
                                try
                                {
                                    return StudentResponseDto.FromEntity(s);
                                }
                                catch (Exception ex)
                                {
                                    logger.LogWarning(ex, "Failed to convert student {StudentId} to DTO", s.Id);
                                    return null;
                                }
                            })
                            .Where(dto => dto != null)
                            .ToList() ?? new List<StudentResponseDto>();

                        processedStudentsByGrade.Add(new StudentsByGradeDto(
                            gradeGroup.GradeId,
                            gradeGroup.GradeName ?? "Unknown Grade",
                            students
                        ));
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to process grade group {GradeId}", gradeGroup.GradeId);
                    }
                }
            }

            // 10. Process subjects with validation
            var processedSubjects = new List<SubjectResponseDto>();
            if (dashboardData.Subjects != null)
            {
                processedSubjects = dashboardData.Subjects
                    .Select(s =>
                    {
                        try
                        {
                            return SubjectResponseDto.FromEntity(s);
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Failed to convert subject {SubjectId} to DTO", s.Id);
                            return null;
                        }
                    })
                    .Where(dto => dto != null)
                    .ToList();
            }

            // 11. Build and return the dashboard DTO
            return new TeacherDashboardDto(
                teacherId,
                $"{teacher.User?.FirstName} {teacher.User?.LastName}".Trim(),
                dashboardData.SubjectsCount,
                dashboardData.TotalStudents,
                dashboardData.TotalMarks,
                dashboardData.TotalAbsences,
                processedSubjects,
                processedStudentsByGrade,
                processedRecentMarks,
                processedRecentAbsences,
                averageMarksBySubjectWithGrades,
                currentSemester.Id,
                currentSemester.Name
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating teacher dashboard for {TeacherId}: {Message}",
                teacherId, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// FIXED: Gets attendance overview with proper validation and error handling
    /// </summary>
    public async Task<AttendanceOverviewDto> GetAttendanceOverviewAsync(Guid teacherId, AttendanceFilterParams filterParams)
    {
        try
        {
            // 1. Validate the teacher exists
            var teacher = await unitOfWork.Teachers.GetByIdAsync(teacherId);
            if (teacher == null)
                throw new NotFoundException("Teacher not found");

            // 2. Get current semester if none specified
            var currentSemester = filterParams.SemesterId.HasValue 
                ? await unitOfWork.Semesters.GetByIdAsync(filterParams.SemesterId.Value)
                : await GetCurrentSemesterAsync(teacher.InstitutionId);

            if (currentSemester == null)
                throw new NotFoundException("No active semester found");

            // 3. Apply semester-based date filtering if no specific dates provided
            var fromDate = filterParams.FromDate ?? currentSemester.StartDate;
            var toDate = filterParams.ToDate ?? (currentSemester.EndDate < DateTime.UtcNow ? currentSemester.EndDate : DateTime.UtcNow);

            // 4. Get attendance data with proper filtering
            var attendanceData = await unitOfWork.Teachers.GetTeacherAttendanceDataAsync(
                teacherId,
                fromDate,
                toDate,
                filterParams.GradeId,
                filterParams.SubjectId,
                currentSemester.Id);

            // 5. Convert to list to avoid multiple enumeration
            var attendanceList = attendanceData?.ToList() ?? new List<Absence>();

            if (!attendanceList.Any())
            {
                return new AttendanceOverviewDto
                {
                    TotalAbsences = 0,
                    RecentAbsences = 0,
                    AbsencesByStatus = new Dictionary<string, int>(),
                    DailyAbsenceTrend = new Dictionary<string, int>(),
                    SemesterId = currentSemester.Id,
                    SemesterName = currentSemester.Name
                };
            }

            // 6. Calculate recent absences (last 7 days)
            var sevenDaysAgo = DateTime.Now.AddDays(-7);
            var recentAbsences = attendanceList.Count(a => a.Date.ToLocalTime() >= sevenDaysAgo);

            // 7. Group by status safely
            var absencesByStatus = attendanceList
                .GroupBy(a => a.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            // 8. Build daily trend with semester context
            int daysToShow = filterParams.DaysToShow ?? 14;
            var dailyTrend = BuildDailyTrend(attendanceList, daysToShow, currentSemester);

            return new AttendanceOverviewDto
            {
                TotalAbsences = attendanceList.Count,
                RecentAbsences = recentAbsences,
                AbsencesByStatus = absencesByStatus,
                DailyAbsenceTrend = dailyTrend,
                SemesterId = currentSemester.Id,
                SemesterName = currentSemester.Name
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving attendance overview for teacher {TeacherId}: {Message}",
                teacherId, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Gets an excused vs unexcused absence breakdown with semester filtering
    /// </summary>
    public async Task<ExcusedUnexcusedBreakdownDto> GetExcusedUnexcusedBreakdownAsync(Guid teacherId, AbsenceBreakdownFilterParams filterParams)
    {
        try
        {
            // Validate teacher exists
            var teacher = await unitOfWork.Teachers.GetByIdAsync(teacherId);
            if (teacher == null)
                throw new NotFoundException("Teacher not found");

            // Get current semester if none specified
            var currentSemester = filterParams.SemesterId.HasValue 
                ? await unitOfWork.Semesters.GetByIdAsync(filterParams.SemesterId.Value)
                : await GetCurrentSemesterAsync(teacher.InstitutionId);

            if (currentSemester == null)
                throw new NotFoundException("No active semester found");

            // Apply semester-based date filtering if no specific dates provided
            var fromDate = filterParams.FromDate ?? currentSemester.StartDate;
            var toDate = filterParams.ToDate ?? (currentSemester.EndDate < DateTime.UtcNow ? currentSemester.EndDate : DateTime.UtcNow);

            // Get the subject breakdown data with semester filtering
            var subjectBreakdown = await unitOfWork.Teachers.GetTeacherAbsencesBreakdownAsync(
                teacherId,
                fromDate,
                toDate,
                filterParams.GradeId,
                filterParams.SubjectId,
                currentSemester.Id);

            if (subjectBreakdown == null || !subjectBreakdown.Any())
                return new ExcusedUnexcusedBreakdownDto
                {
                    ExcusedCount = 0,
                    UnexcusedCount = 0,
                    ExcusedPercentage = 0,
                    UnexcusedPercentage = 0,
                    SubjectBreakdown = new List<SubjectExcusedUnexcusedDto>(),
                    SemesterId = currentSemester.Id,
                    SemesterName = currentSemester.Name
                };

            // Sum up totals from the subject breakdown
            int excusedCount = subjectBreakdown.Sum(s => s.ExcusedCount);
            int unexcusedCount = subjectBreakdown.Sum(s => s.UnexcusedCount);
            int totalCount = excusedCount + unexcusedCount;

            // Calculate percentages
            decimal excusedPercentage = totalCount > 0 ? Math.Round((decimal)excusedCount / totalCount * 100, 1) : 0;
            decimal unexcusedPercentage = totalCount > 0 ? Math.Round((decimal)unexcusedCount / totalCount * 100, 1) : 0;

            return new ExcusedUnexcusedBreakdownDto
            {
                ExcusedCount = excusedCount,
                UnexcusedCount = unexcusedCount,
                ExcusedPercentage = excusedPercentage,
                UnexcusedPercentage = unexcusedPercentage,
                SubjectBreakdown = subjectBreakdown.ToList(),
                SemesterId = currentSemester.Id,
                SemesterName = currentSemester.Name
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving excused/unexcused breakdown for teacher {TeacherId}: {Message}",
                teacherId, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// FIXED: Gets at-risk students with proper validation and calculations
    /// </summary>
    public async Task<AtRiskStudentsResponseDto> GetAtRiskStudentsAsync(Guid teacherId, AtRiskStudentsFilterParams filterParams)
    {
        try
        {
            // 1. Validate teacher exists
            var teacher = await unitOfWork.Teachers.GetByIdAsync(teacherId);
            if (teacher == null)
                throw new NotFoundException("Teacher not found");

            // 2. Get current semester if none specified
            var currentSemester = filterParams.SemesterId.HasValue 
                ? await unitOfWork.Semesters.GetByIdAsync(filterParams.SemesterId.Value)
                : await GetCurrentSemesterAsync(teacher.InstitutionId);

            if (currentSemester == null)
                throw new NotFoundException("No active semester found");

            // 3. Calculate semester-specific class days if not provided
            var totalClassDays = filterParams.TotalClassDays ?? CalculateSemesterClassDays(currentSemester);

            // 4. Apply semester-based date filtering if no specific dates provided
            var fromDate = filterParams.FromDate ?? currentSemester.StartDate;
            var toDate = filterParams.ToDate ?? (currentSemester.EndDate < DateTime.UtcNow ? currentSemester.EndDate : DateTime.UtcNow);

            // 5. Get students and their absence data with semester filtering
            var studentAbsenceData = await unitOfWork.Teachers.GetStudentAbsenceDataAsync(
                teacherId,
                filterParams.GradeId,
                filterParams.SubjectId,
                fromDate,
                toDate,
                currentSemester.Id);

            // 6. Convert to list to avoid multiple enumeration
            var studentList = studentAbsenceData?.ToList() ?? new List<AtRiskStudentDto>();

            if (!studentList.Any())
            {
                return new AtRiskStudentsResponseDto
                {
                    HighRiskThreshold = filterParams.HighRiskThreshold ?? 20,
                    MediumRiskThreshold = filterParams.MediumRiskThreshold ?? 10,
                    TotalClassDays = totalClassDays,
                    AtRiskStudents = new List<AtRiskStudentDto>(),
                    SemesterId = currentSemester.Id,
                    SemesterName = currentSemester.Name
                };
            }

            // 7. Determine risk thresholds
            int highRiskThreshold = filterParams.HighRiskThreshold ?? 20;
            int mediumRiskThreshold = filterParams.MediumRiskThreshold ?? 10;

            // 8. Calculate at-risk students with proper validation
            var atRiskStudents = new List<AtRiskStudentDto>();

            foreach (var student in studentList)
            {
                try
                {
                    // Calculate absence rate based on semester class days
                    decimal absenceRate = totalClassDays > 0
                        ? Math.Round((decimal)student.TotalAbsences / totalClassDays * 100, 1)
                        : 0;

                    // Determine risk level
                    string riskLevel = "none";
                    if (absenceRate >= highRiskThreshold)
                        riskLevel = "high";
                    else if (absenceRate >= mediumRiskThreshold)
                        riskLevel = "medium";

                    // Only include at-risk students
                    if (riskLevel == "high" || riskLevel == "medium")
                    {
                        student.AbsenceRate = absenceRate;
                        student.RiskLevel = riskLevel;
                        atRiskStudents.Add(student);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to process at-risk student {StudentId}", student.Id);
                }
            }

            // 9. Sort by absence rate descending
            atRiskStudents = atRiskStudents.OrderByDescending(s => s.AbsenceRate).ToList();

            return new AtRiskStudentsResponseDto
            {
                HighRiskThreshold = highRiskThreshold,
                MediumRiskThreshold = mediumRiskThreshold,
                TotalClassDays = totalClassDays,
                AtRiskStudents = atRiskStudents,
                SemesterId = currentSemester.Id,
                SemesterName = currentSemester.Name
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving at-risk students for teacher {TeacherId}: {Message}",
                teacherId, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Gets aggregated attendance statistics with semester filtering and grading system integration
    /// </summary>
    public async Task<AttendanceStatisticsDto> GetAttendanceStatisticsAsync(Guid teacherId, StatisticsFilterParams filterParams)
    {
        try
        {
            // Validate teacher exists
            var teacher = await unitOfWork.Teachers.GetByIdAsync(teacherId) ?? throw new NotFoundException("Teacher not found");

            // Get current semester if none specified
            var currentSemester = filterParams.SemesterId.HasValue 
                ? await unitOfWork.Semesters.GetByIdAsync(filterParams.SemesterId.Value)
                : await GetCurrentSemesterAsync(teacher.InstitutionId);

            if (currentSemester == null)
                throw new NotFoundException("No active semester found");

            // Apply semester-based date filtering if no specific dates provided
            var fromDate = filterParams.FromDate ?? currentSemester.StartDate;
            var toDate = filterParams.ToDate ?? (currentSemester.EndDate < DateTime.UtcNow ? currentSemester.EndDate : DateTime.UtcNow);

            // Get attendance statistics data with semester filtering
            var statistics = await unitOfWork.Teachers.GetAttendanceStatisticsAsync(
                teacherId,
                fromDate,
                toDate,
                filterParams.GradeId,
                filterParams.SubjectId,
                currentSemester.Id);

            if (statistics == null)
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
                    TrendByMonth = new Dictionary<string, int>(),
                    SemesterId = currentSemester.Id,
                    SemesterName = currentSemester.Name
                };

            // Enhance statistics with semester context
            statistics.SemesterId = currentSemester.Id;
            statistics.SemesterName = currentSemester.Name;

            return statistics;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving attendance statistics for teacher {TeacherId}: {Message}",
                teacherId, ex.Message);
            throw;
        }
    }

    // Grade assignment methods remain the same...
    public async Task<TeacherGradeAssignmentResultDto> AssignTeacherToGradesAsync(Guid teacherId, AssignTeacherToGradesDto dto)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            // Verify teacher exists
            var teacher = await unitOfWork.Teachers.GetByIdAsync(teacherId) 
                ?? throw new NotFoundException("Teacher not found");

            // Verify all grades exist and belong to same institution as teacher
            var grades = await unitOfWork.Grades.GetByIdsAsync(dto.GradeIds);
            var invalidGrades = dto.GradeIds.Except(grades.Select(g => g.Id)).ToList();
            
            if (invalidGrades.Any())
                throw new NotFoundException($"Grades not found: {string.Join(", ", invalidGrades)}");

            // Verify grades belong to teacher's institution
            var invalidInstitutionGrades = grades.Where(g => g.InstitutionId != teacher.InstitutionId).ToList();
            if (invalidInstitutionGrades.Any())
                throw new ValidationException("Cannot assign teacher to grades from different institutions");

            // Assign grades to teacher
            await unitOfWork.Teachers.AssignGradesAsync(teacherId, dto.GradeIds);
            
            await unitOfWork.CommitAsync();

            // Calculate impact
            var totalStudents = grades.Sum(g => g.Students?.Count ?? 0);
            
            logger.LogInformation("Assigned teacher {TeacherId} to grades: {GradeIds}", 
                teacherId, string.Join(", ", dto.GradeIds));

            return new TeacherGradeAssignmentResultDto(
                Success: true,
                Message: $"Successfully assigned teacher to {dto.GradeIds.Count()} grade(s)",
                TeacherId: teacherId,
                AssignedGradeIds: dto.GradeIds,
                TotalGradesAssigned: dto.GradeIds.Count(),
                TotalStudentsImpacted: totalStudents
            );
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error assigning teacher {TeacherId} to grades: {Message}", teacherId, ex.Message);
            
            if (ex is NotFoundException || ex is ValidationException)
                throw;
                
            throw new ApplicationException("An error occurred while assigning teacher to grades.", ex);
        }
    }

    public async Task<TeacherGradeAssignmentResultDto> UnassignTeacherFromGradesAsync(Guid teacherId, UnassignTeacherFromGradesDto dto)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            // Verify teacher exists
            var teacher = await unitOfWork.Teachers.GetByIdAsync(teacherId) 
                ?? throw new NotFoundException("Teacher not found");

            // Get current assignments to verify they exist
            var currentGrades = await unitOfWork.Teachers.GetAssignedGradesAsync(teacherId);
            var currentGradeIds = currentGrades.Select(g => g.Id).ToHashSet();
            
            var invalidUnassignments = dto.GradeIds.Where(id => !currentGradeIds.Contains(id)).ToList();
            if (invalidUnassignments.Any())
                throw new ValidationException($"Teacher is not assigned to grades: {string.Join(", ", invalidUnassignments)}");

            // Unassign grades from teacher
            await unitOfWork.Teachers.UnassignGradesAsync(teacherId, dto.GradeIds);
            
            await unitOfWork.CommitAsync();

            // Calculate impact
            var removedGrades = currentGrades.Where(g => dto.GradeIds.Contains(g.Id));
            var totalStudents = removedGrades.Sum(g => g.Students?.Count ?? 0);
            
            logger.LogInformation("Unassigned teacher {TeacherId} from grades: {GradeIds}", 
                teacherId, string.Join(", ", dto.GradeIds));

            return new TeacherGradeAssignmentResultDto(
                Success: true,
                Message: $"Successfully unassigned teacher from {dto.GradeIds.Count()} grade(s)",
                TeacherId: teacherId,
                AssignedGradeIds: dto.GradeIds,
                TotalGradesAssigned: dto.GradeIds.Count(),
                TotalStudentsImpacted: totalStudents
            );
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error unassigning teacher {TeacherId} from grades: {Message}", teacherId, ex.Message);
            
            if (ex is NotFoundException || ex is ValidationException)
                throw;
                
            throw new ApplicationException("An error occurred while unassigning teacher from grades.", ex);
        }
    }

    public async Task<TeacherWithGradeAssignmentsResponseDto> GetTeacherWithGradeAssignmentsAsync(Guid teacherId)
    {
        try
        {
            // Get teacher with all related data
            var teacher = await unitOfWork.Teachers.GetTeacherWithGradesAsync(teacherId) 
                ?? throw new NotFoundException("Teacher not found");

            // Get assigned grades (excluding homeroom which is handled separately)
            var assignedGrades = await unitOfWork.Teachers.GetAssignedGradesAsync(teacherId);
            var assignedSubjects = teacher.Subjects ?? new List<Subject>();

            return TeacherWithGradeAssignmentsResponseDto.FromEntity(teacher, teacher.User!, assignedGrades, assignedSubjects);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving teacher with grade assignments {TeacherId}: {Message}",
                teacherId, ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<GradeAssignmentResponseDto>> GetTeacherAssignedGradesAsync(Guid teacherId)
    {
        try
        {
            // Verify teacher exists
            var teacher = await unitOfWork.Teachers.GetByIdAsync(teacherId) 
                ?? throw new NotFoundException("Teacher not found");

            // Get all grades assigned to teacher
            var assignedGrades = await unitOfWork.Teachers.GetAssignedGradesAsync(teacherId);
            
            return assignedGrades.Select(g => GradeAssignmentResponseDto.FromEntity(g, g.Id == teacher.ClassGradeId));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assigned grades for teacher {TeacherId}: {Message}",
                teacherId, ex.Message);
            throw;
        }
    }

    public async Task<TeacherGradeAssignmentResultDto> UpdateTeacherGradeAssignmentsAsync(Guid teacherId, UpdateTeacherGradeAssignmentsDto dto)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            // Verify teacher exists
            var teacher = await unitOfWork.Teachers.GetByIdAsync(teacherId) 
                ?? throw new NotFoundException("Teacher not found");

            // Verify all grades exist and belong to same institution
            if (dto.GradeIds.Any())
            {
                var grades = await unitOfWork.Grades.GetByIdsAsync(dto.GradeIds);
                var invalidGrades = dto.GradeIds.Except(grades.Select(g => g.Id)).ToList();
                
                if (invalidGrades.Any())
                    throw new NotFoundException($"Grades not found: {string.Join(", ", invalidGrades)}");

                var invalidInstitutionGrades = grades.Where(g => g.InstitutionId != teacher.InstitutionId).ToList();
                if (invalidInstitutionGrades.Any())
                    throw new ValidationException("Cannot assign teacher to grades from different institutions");
            }

            // Get current assignments for comparison
            var currentGrades = await unitOfWork.Teachers.GetAssignedGradesAsync(teacherId);
            var currentGradeIds = currentGrades.Select(g => g.Id).ToHashSet();
            var newGradeIds = dto.GradeIds.ToHashSet();

            // Update assignments
            await unitOfWork.Teachers.UpdateGradeAssignmentsAsync(teacherId, dto.GradeIds);
            
            await unitOfWork.CommitAsync();

            // Calculate impact
            var addedGrades = newGradeIds.Except(currentGradeIds);
            var removedGrades = currentGradeIds.Except(newGradeIds);
            var finalGrades = await unitOfWork.Grades.GetByIdsAsync(dto.GradeIds);
            var totalStudents = finalGrades.Sum(g => g.Students?.Count ?? 0);
            
            logger.LogInformation("Updated grade assignments for teacher {TeacherId}. Added: {AddedGrades}, Removed: {RemovedGrades}", 
                teacherId, string.Join(", ", addedGrades), string.Join(", ", removedGrades));

            return new TeacherGradeAssignmentResultDto(
                Success: true,
                Message: $"Successfully updated teacher assignments to {dto.GradeIds.Count()} grade(s)",
                TeacherId: teacherId,
                AssignedGradeIds: dto.GradeIds,
                TotalGradesAssigned: dto.GradeIds.Count(),
                TotalStudentsImpacted: totalStudents
            );
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error updating teacher grade assignments: {Message}", ex.Message);
            
            if (ex is NotFoundException || ex is ValidationException)
                throw;
                
            throw new ApplicationException("An error occurred while updating teacher grade assignments.", ex);
        }
    }

    public async Task<TeacherGradeAssignmentSummaryDto> GetTeacherGradeAssignmentSummaryAsync(Guid teacherId)
    {
        try
        {
            // Get teacher with basic info
            var teacher = await unitOfWork.Teachers.GetByIdWithUserAsync(teacherId) 
                ?? throw new NotFoundException("Teacher not found");

            // Get all assigned grades
            var assignedGrades = await unitOfWork.Teachers.GetAssignedGradesAsync(teacherId);

            return TeacherGradeAssignmentSummaryDto.FromEntity(teacher, assignedGrades);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving teacher grade assignment summary {TeacherId}: {Message}",
                teacherId, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Helper method to get the current active semester for an institution
    /// </summary>
    private async Task<Semester?> GetCurrentSemesterAsync(Guid institutionId)
    {
        return await unitOfWork.Semesters.GetCurrentActiveAsync(institutionId);
    }

    /// <summary>
    /// Helper method to get the institution's default grading system
    /// </summary>
    private async Task<GradingSystem> GetInstitutionGradingSystemAsync(Guid institutionId)
    {
        var gradingSystemDto = await gradingSystemService.GetDefaultForInstitutionAsync(institutionId);
        return await unitOfWork.GradingSystems.GetWithGradeScalesAsync(gradingSystemDto.Id);
    }

    /// <summary>
    /// Helper method to enhance mark DTOs with grading system information
    /// </summary>
    private MarkResponseDto EnhanceMarkWithGradingSystem(Mark mark, GradingSystem gradingSystem)
    {
        var markDto = MarkResponseDto.FromEntity(mark);
        
        // Add grading system enhanced fields
        markDto.Grade = gradingSystemService.ConvertScoreToGrade(mark.Value, gradingSystem.Id);
        markDto.GpaPoints = gradingSystemService.ConvertScoreToGpaPoints(mark.Value, gradingSystem.Id);
        markDto.Status = gradingSystemService.DetermineStatus(mark.Value, gradingSystem.Id);
        
        return markDto;
    }

    /// <summary>
    /// Helper method to calculate semester-specific class days
    /// </summary>
    private int CalculateSemesterClassDays(Semester semester)
    {
        var startDate = semester.StartDate;
        var endDate = semester.EndDate < DateTime.UtcNow ? semester.EndDate : DateTime.UtcNow;
        
        var businessDays = 0;
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                businessDays++;
        }
        
        return Math.Max(1, businessDays);
    }

    /// <summary>
    /// Helper method to build daily trend data with semester context
    /// </summary>
    private Dictionary<string, int> BuildDailyTrend(IEnumerable<Absence> absences, int daysToShow, Semester semester)
    {
        var trendByDay = new Dictionary<string, int>();
        var now = DateTime.Now;
        var endDate = semester.EndDate < now ? semester.EndDate.ToLocalTime() : now;
        var startDate = endDate.AddDays(-daysToShow).Date;

        // Ensure we don't go before semester start
        if (startDate < semester.StartDate.ToLocalTime().Date)
            startDate = semester.StartDate.ToLocalTime().Date;

        for (var date = startDate; date <= endDate.Date; date = date.AddDays(1))
        {
            var dateStr = date.ToString("MM/dd");
            trendByDay[dateStr] = absences.Count(a => a.Date.ToLocalTime().Date == date.Date);
        }

        return trendByDay;
    }
}