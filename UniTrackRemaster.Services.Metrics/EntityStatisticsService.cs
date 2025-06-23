using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Metrics;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Services.Organization;

namespace UniTrackRemaster.Services.Metrics;


public class EntityStatisticsService(
    IUnitOfWork unitOfWork,
    ILogger<EntityStatisticsService> logger,
    IGradingSystemService gradingSystemService) : IEntityStatisticsService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<EntityStatisticsService> _logger = logger;
    private readonly IGradingSystemService _gradingSystemService = gradingSystemService;

    // Cache statistics for 5 minutes to avoid excessive database queries
    private UserStatisticsDto? _cachedUserStats;
    private AcademicStatisticsDto? _cachedAcademicStats;
    private ActivityStatisticsDto? _cachedActivityStats;
    private DateTime _lastUserStatsRefresh = DateTime.MinValue;
    private DateTime _lastAcademicStatsRefresh = DateTime.MinValue;
    private DateTime _lastActivityStatsRefresh = DateTime.MinValue;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);

    public async Task<UserStatisticsDto> GetUserStatisticsAsync(CancellationToken cancellationToken = default)
    {
        // Return cached data if available and not expired
        if (_cachedUserStats != null && DateTime.UtcNow - _lastUserStatsRefresh < _cacheTimeout)
        {
            return _cachedUserStats;
        }

        try
        {
            _logger.LogInformation("Collecting user statistics from database");

            // Get counts from repositories
            var adminCount = await _unitOfWork.Admins.CountAsync(cancellationToken);
            var teacherCount = await _unitOfWork.Teachers.CountAsync(cancellationToken);
            var studentCount = await _unitOfWork.Students.CountAsync(cancellationToken);
            // var parentCount = await _unitOfWork.Parents.CountAll(cancellationToken);

            // Calculate active users
            var activeAdmins = await _unitOfWork.Admins.CountAsync(a => a.Status == ProfileStatus.Active, cancellationToken);
            var activeUsers = activeAdmins;

            //TODO: Add parents later
            var totalUsers = adminCount + teacherCount + studentCount;

            // Create role distribution dictionary
            var usersByRole = new Dictionary<string, int>
                {
                    { "Admin", adminCount },
                    { "Teacher", teacherCount },
                    { "Student", studentCount },
                    // { "Parent", parentCount }
                };

            // Create and cache the result
            _cachedUserStats = new UserStatisticsDto(
                TotalUsers: totalUsers,
                ActiveUsers: activeUsers,
                AdminCount: adminCount,
                TeacherCount: teacherCount,
                StudentCount: studentCount,
                ParentCount: 0,
                UsersByRole: usersByRole
            );

            _lastUserStatsRefresh = DateTime.UtcNow;
            return _cachedUserStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting user statistics");
            throw;
        }
    }

    public async Task<AcademicStatisticsDto> GetAcademicStatisticsAsync(CancellationToken cancellationToken = default)
    {
        // Return cached data if available and not expired
        if (_cachedAcademicStats != null && DateTime.UtcNow - _lastAcademicStatsRefresh < _cacheTimeout)
        {
            return _cachedAcademicStats;
        }

        try
        {
            _logger.LogInformation("Collecting academic statistics from database");

            // Get counts from repositories
            var institutionCount = await _unitOfWork.Institutions.CountAsync(cancellationToken);
            var schoolCount = await _unitOfWork.Schools.CountAsync(cancellationToken);
            var universityCount = await _unitOfWork.Universities.CountAsync(cancellationToken);
            var facultyCount = await _unitOfWork.Faculties.CountAsync(cancellationToken);
            var departmentCount = await _unitOfWork.Departments.CountAsync(cancellationToken);
            var majorCount = await _unitOfWork.Majors.CountAsync(cancellationToken);
            var subjectCount = await _unitOfWork.Subjects.CountAsync(cancellationToken);
            var gradeCount = await _unitOfWork.Grades.CountAsync(cancellationToken);

            // Get institution types distribution
            var institutions = await _unitOfWork.Institutions.GetAllAsync(cancellationToken);
            var institutionsByType = institutions
                .GroupBy(i => i.Type.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            // Create and cache the result
            _cachedAcademicStats = new AcademicStatisticsDto(
                TotalInstitutions: institutionCount,
                SchoolCount: schoolCount,
                UniversityCount: universityCount,
                FacultyCount: facultyCount,
                DepartmentCount: departmentCount,
                MajorCount: majorCount,
                SubjectCount: subjectCount,
                GradeCount: gradeCount,
                InstitutionsByType: institutionsByType
            );

            _lastAcademicStatsRefresh = DateTime.UtcNow;
            return _cachedAcademicStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting academic statistics");
            throw;
        }
    }

    public async Task<ActivityStatisticsDto> GetActivityStatisticsAsync(CancellationToken cancellationToken = default)
    {
        // Return cached data if available and not expired
        if (_cachedActivityStats != null && DateTime.UtcNow - _lastActivityStatsRefresh < _cacheTimeout)
        {
            return _cachedActivityStats;
        }

        try
        {
            _logger.LogInformation("Collecting activity statistics from database");

            // Get counts from repositories
            var attendanceCount = await _unitOfWork.Absences.CountAsync(cancellationToken);
            var markCount = await _unitOfWork.Grades.CountAsync(cancellationToken);
            var applicationCount = await _unitOfWork.Applications.CountAsync(cancellationToken);

            // Get attendance status distribution
            var attendances = await _unitOfWork.Absences.GetAllAsync(cancellationToken);
            var attendanceByStatus = attendances
                .GroupBy(a => a.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            // Get application status distribution
            var applications = await _unitOfWork.Applications.GetAllAsync(cancellationToken);
            var applicationsByStatus = applications
                .GroupBy(a => a.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            // Get the default grading system for each institution when calculating marks
            var institutions = await _unitOfWork.Institutions.GetAllAsync(cancellationToken);
            var defaultGradingSystems = new Dictionary<Guid, Guid>();

            foreach (var institution in institutions)
            {
                try
                {
                    var defaultGradingSystem = await _gradingSystemService.GetDefaultForInstitutionAsync(institution.Id, cancellationToken);
                    if (defaultGradingSystem != null)
                    {
                        defaultGradingSystems[institution.Id] = defaultGradingSystem.Id;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get default grading system for institution {InstitutionId}", institution.Id);
                }
            }

            // Get average marks by subject
            var marks = await _unitOfWork.Grades.GetAllAsync(cancellationToken);
            var marksBySubject = new Dictionary<string, double>();

            // Create and cache the result
            _cachedActivityStats = new ActivityStatisticsDto(
                TotalAttendances: attendanceCount,
                TotalMarks: markCount,
                TotalApplications: applicationCount,
                AttendanceByStatus: attendanceByStatus,
                ApplicationsByStatus: applicationsByStatus
            );

            _lastActivityStatsRefresh = DateTime.UtcNow;
            return _cachedActivityStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting activity statistics");
            throw;
        }
    }

    public async Task<SystemStatisticsDto> GetSystemStatisticsAsync(CancellationToken cancellationToken = default)
    {
        // Execute requests sequentially instead of in parallel to avoid DbContext concurrency issues
        var userStats = await GetUserStatisticsAsync(cancellationToken);
        var academicStats = await GetAcademicStatisticsAsync(cancellationToken);
        var activityStats = await GetActivityStatisticsAsync(cancellationToken);

        return new SystemStatisticsDto(
            CollectedAt: DateTimeOffset.UtcNow,
            Users: userStats,
            Academic: academicStats,
            Activity: activityStats
        );
    }


    public async Task<EntityCountsDto> GetAllEntityCountsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Collecting entity counts from database");
            var entities = new List<EntityCountDto>();


            // Institution stats
            entities.Add(new EntityCountDto(
                EntityName: "Institution",
                TotalCount: await _unitOfWork.Institutions.CountAsync(cancellationToken),
                ActiveCount: await _unitOfWork.Institutions.CountAsync(e => e.IntegrationStatus == IntegrationStatus.Active, cancellationToken),
                LastUpdated: DateTime.UtcNow
            ));

            // School stats
            entities.Add(new EntityCountDto(
                EntityName: "School",
                TotalCount: await _unitOfWork.Schools.CountAsync(cancellationToken),
                ActiveCount: await _unitOfWork.Schools.CountAsync(cancellationToken),
                LastUpdated: DateTime.UtcNow
            ));

            // University stats
            entities.Add(new EntityCountDto(
                EntityName: "University",
                TotalCount: await _unitOfWork.Universities.CountAsync(cancellationToken),
                ActiveCount: await _unitOfWork.Universities.CountAsync(cancellationToken),
                LastUpdated: DateTime.UtcNow
            ));

            // Faculty stats
            entities.Add(new EntityCountDto(
                EntityName: "Faculty",
                TotalCount: await _unitOfWork.Faculties.CountAsync(cancellationToken),
                ActiveCount: await _unitOfWork.Faculties.CountAsync(f => f.Status == FacultyStatus.Active, cancellationToken),
                LastUpdated: DateTime.UtcNow
            ));

            // Department stats
            entities.Add(new EntityCountDto(
                EntityName: "Department",
                TotalCount: await _unitOfWork.Departments.CountAsync(cancellationToken),
                ActiveCount: await _unitOfWork.Departments.CountAsync(d => d.Status == DepartmentStatus.Active, cancellationToken),
                LastUpdated: DateTime.UtcNow
            ));

            // Subject stats
            entities.Add(new EntityCountDto(
                EntityName: "Subject",
                TotalCount: await _unitOfWork.Subjects.CountAsync(cancellationToken),
                ActiveCount: await _unitOfWork.Subjects.CountAsync(cancellationToken),
                LastUpdated: DateTime.UtcNow
            ));

            // Student stats
            entities.Add(new EntityCountDto(
                EntityName: "Student",
                TotalCount: await _unitOfWork.Students.CountAsync(cancellationToken),
                ActiveCount: await _unitOfWork.Students.CountAsync(cancellationToken),
                LastUpdated: DateTime.UtcNow
            ));

            // Teacher stats
            entities.Add(new EntityCountDto(
                EntityName: "Teacher",
                TotalCount: await _unitOfWork.Teachers.CountAsync(cancellationToken),
                ActiveCount: await _unitOfWork.Teachers.CountAsync(cancellationToken),
                LastUpdated: DateTime.UtcNow
            ));

            // Admin stats
            entities.Add(new EntityCountDto(
                EntityName: "Admin",
                TotalCount: await _unitOfWork.Admins.CountAsync(cancellationToken),
                ActiveCount: await _unitOfWork.Admins.CountAsync(a => a.Status == ProfileStatus.Active, cancellationToken),
                LastUpdated: DateTime.UtcNow
            ));

            return new EntityCountsDto(
                CollectedAt: DateTimeOffset.UtcNow,
                Entities: entities
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting entity counts");
            throw;
        }
    }

    // Helper methods

    private async Task<EntityCountDto> GetEntityCountAsync(
        string entityName,
        Func<CancellationToken, Task<int>> totalCountFunc,
        Task<int> activeCountTask,
        CancellationToken cancellationToken)
    {
        var totalCount = await totalCountFunc(cancellationToken);

        var activeCount = await activeCountTask;

        return new EntityCountDto(
            EntityName: entityName,
            TotalCount: totalCount,
            ActiveCount: activeCount,
            LastUpdated: DateTime.UtcNow
        );
    }
}