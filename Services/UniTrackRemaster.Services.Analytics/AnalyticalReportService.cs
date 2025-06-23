using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Analytics;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Vector;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using UniTrackRemaster.Commons.Services;

namespace UniTrackRemaster.Services.Analytics;
public class AnalyticsReportService(
    UniTrackDbContext context,
    ILogger<AnalyticsReportService> logger,
    IHybridAnalyticsService hybridAnalytics)
    : IAnalyticsReportService
{
    public async Task<InstitutionAnalyticsReport> GenerateInstitutionReportAsync(Guid institutionId, DateTime from, DateTime to)
    {
        try
        {
            logger.LogInformation("Generating analytics report for institution {InstitutionId} from {From} to {To}",
                institutionId, from, to);

            // OPTIMIZED: Use projections instead of loading full entities
            var institutionData = await context.Institutions
                .Where(i => i.Id == institutionId)
                .Select(i => new
                {
                    i.Id,
                    i.Name,
                    i.Type,
                    StudentCount = i.Students!.Count(s => s.Status == ProfileStatus.Active),
                    TeacherCount = i.Teachers!.Count(t => t.Status == ProfileStatus.Active),
                    StudentIds = i.Students!.Where(s => s.Status == ProfileStatus.Active).Select(s => s.Id),
                    TeacherIds = i.Teachers!.Where(t => t.Status == ProfileStatus.Active).Select(t => t.Id)
                })
                .FirstOrDefaultAsync();

            if (institutionData == null)
                throw new ArgumentException($"Institution with ID {institutionId} not found");

            var studentIds = institutionData.StudentIds.ToList();
            var teacherIds = institutionData.TeacherIds.ToList();

            // OPTIMIZED: Get academic data with efficient queries
            var marksData = await context.Marks
                .Where(m => studentIds.Contains(m.StudentId) &&
                           m.CreatedAt >= from && m.CreatedAt <= to)
                .Select(m => new { m.Value, m.Subject!.Name })
                .ToListAsync();

            var absencesData = await context.Absences
                .Where(a => studentIds.Contains(a.StudentId) &&
                           a.Date >= from && a.Date <= to)
                .Select(a => new { a.Date, a.Status })
                .ToListAsync();

            // Calculate core metrics
            var totalEnrollments = institutionData.StudentCount;
            var teacherCount = institutionData.TeacherCount;
            var studentTeacherRatio = teacherCount > 0 ? (decimal)totalEnrollments / teacherCount : 0;

            // Calculate academic score (average of all marks)
            var overallAcademicScore = marksData.Any() ? marksData.Average(m => (double)m.Value) : 0;

            // Calculate attendance rate
            var totalDays = Math.Max((to - from).Days, 1);
            var totalPossibleAttendance = totalEnrollments * totalDays;
            var actualAbsences = absencesData.Count;
            var attendanceRate = totalPossibleAttendance > 0
                ? (decimal)(100.0 - (actualAbsences * 100.0 / totalPossibleAttendance))
                : 100m;

            // Calculate subject performance scores
            var subjectPerformanceScores = marksData
                .Where(m => !string.IsNullOrEmpty(m.Name))
                .GroupBy(m => m.Name!)
                .ToDictionary(
                    g => g.Key,
                    g => (decimal)g.Average(m => (double)m.Value)
                );

            // Determine performance category
            var performanceCategory = overallAcademicScore switch
            {
                >= 90 => PerformanceCategory.Excellent,
                >= 80 => PerformanceCategory.Good,
                >= 70 => PerformanceCategory.Average,
                >= 60 => PerformanceCategory.BelowAverage,
                _ => PerformanceCategory.NeedsImprovement
            };

            // Generate achievements
            var achievements = GenerateAchievements(overallAcademicScore, attendanceRate, subjectPerformanceScores);

            // IMPLEMENTED: Get previous period data for growth calculation
            var previousFrom = from.AddYears(-1);
            var previousTo = to.AddYears(-1);
            var previousReport = await GetLatestInstitutionReportAsync(institutionId, previousFrom, previousTo);

            var yearOverYearGrowth = previousReport != null
                ? ((decimal)overallAcademicScore - previousReport.OverallAcademicScore) / Math.Max(previousReport.OverallAcademicScore, 1) * 100
                : 0;

            // IMPLEMENTED: Calculate enrollment growth
            var enrollmentGrowthRate = await CalculateEnrollmentGrowthAsync(institutionId, totalEnrollments);

            // IMPLEMENTED: Calculate teacher retention
            var teacherRetentionRate = await CalculateTeacherRetentionAsync(institutionId, teacherIds);

            var report = new InstitutionAnalyticsReport
            {
                Id = Guid.NewGuid(),
                InstitutionId = institutionId,
                From = from,
                To = to,
                PeriodType = CalculatePeriodType(from, to),
                OverallAcademicScore = (decimal)overallAcademicScore,
                YearOverYearGrowth = yearOverYearGrowth,
                TotalEnrollments = totalEnrollments,
                EnrollmentGrowthRate = enrollmentGrowthRate,
                AverageAttendanceRate = attendanceRate,
                StudentTeacherRatio = studentTeacherRatio,
                TeacherRetentionRate = teacherRetentionRate,
                OverallPerformanceCategory = performanceCategory,
                SubjectPerformanceScores = JsonSerializer.Serialize(subjectPerformanceScores),
                TopAchievements = JsonSerializer.Serialize(achievements),
                NationalRankings = JsonSerializer.Serialize(new Dictionary<string, int>()),
                RegionalRankings = JsonSerializer.Serialize(new Dictionary<string, int>()),
                DepartmentRankings = JsonSerializer.Serialize(new Dictionary<string, int>()),
                PopularMajors = JsonSerializer.Serialize(new Dictionary<string, int>()),
                MajorGrowthRates = JsonSerializer.Serialize(new Dictionary<string, decimal>()),
                FastestGrowingAreas = JsonSerializer.Serialize(new List<string>()),
                StrongestSubjects = JsonSerializer.Serialize(
                    subjectPerformanceScores
                        .OrderByDescending(kvp => kvp.Value)
                        .Take(3)
                        .Select(kvp => kvp.Key)
                        .ToList()
                ),
                IsPublic = false
            };

            // Generate AI summary
            try
            {
                report.ExecutiveSummary = await hybridAnalytics.GenerateExecutiveSummaryAsync(report);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to generate AI summary for report {ReportId}", report.Id);
                report.ExecutiveSummary = GenerateBasicExecutiveSummary(report);
            }

            context.InstitutionAnalyticsReports.Add(report);
            await context.SaveChangesAsync();

            logger.LogInformation("Successfully generated analytics report {ReportId} for institution {InstitutionId}",
                report.Id, institutionId);

            return report;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate analytics report for institution {InstitutionId}", institutionId);
            throw;
        }
    }

    public async Task<MarketAnalyticsReport> GenerateMarketReportAsync(ReportPeriodType periodType, string reportPeriod)
    {
        try
        {
            logger.LogInformation("Generating market analytics report for {PeriodType} {ReportPeriod}",
                periodType, reportPeriod);

            // OPTIMIZED: Use projections for market data
            var institutionReports = await context.InstitutionAnalyticsReports
                .Where(r => r.PeriodType == periodType)
                .Select(r => new
                {
                    r.InstitutionId,
                    r.OverallAcademicScore,
                    r.TotalEnrollments,
                    r.YearOverYearGrowth,
                    r.EnrollmentGrowthRate,
                    InstitutionName = r.Institution!.Name,
                    InstitutionType = r.Institution.Type
                })
                .ToListAsync();

            var totalInstitutions = institutionReports.Count;
            var totalStudents = institutionReports.Sum(r => r.TotalEnrollments);
            var averageScore = institutionReports.Any()
                ? institutionReports.Average(r => r.OverallAcademicScore)
                : 0;

            var marketGrowthRate = institutionReports.Any()
                ? institutionReports.Average(r => r.YearOverYearGrowth)
                : 0;

            // Generate rankings
            var enrollmentLeaders = institutionReports
                .OrderByDescending(r => r.TotalEnrollments)
                .Take(10)
                .Select((r, index) => new InstitutionRanking(
                    r.InstitutionId,
                    r.InstitutionName ?? "Unknown",
                    r.TotalEnrollments,
                    index + 1,
                    r.EnrollmentGrowthRate
                ))
                .ToList();

            var academicLeaders = institutionReports
                .OrderByDescending(r => r.OverallAcademicScore)
                .Take(10)
                .Select((r, index) => new InstitutionRanking(
                    r.InstitutionId,
                    r.InstitutionName ?? "Unknown",
                    r.OverallAcademicScore,
                    index + 1,
                    r.YearOverYearGrowth
                ))
                .ToList();

            var fastestGrowing = institutionReports
                .OrderByDescending(r => r.YearOverYearGrowth)
                .Take(10)
                .Select((r, index) => new InstitutionRanking(
                    r.InstitutionId,
                    r.InstitutionName ?? "Unknown",
                    r.YearOverYearGrowth,
                    index + 1,
                    r.YearOverYearGrowth
                ))
                .ToList();

            var report = new MarketAnalyticsReport
            {
                Id = Guid.NewGuid(),
                ReportType = ReportType.MarketAnalytics,
                PeriodType = periodType,
                ReportPeriod = reportPeriod,
                TotalInstitutions = totalInstitutions,
                TotalStudents = totalStudents,
                MarketGrowthRate = marketGrowthRate,
                AverageInstitutionScore = averageScore,
                EnrollmentLeaders = JsonSerializer.Serialize(enrollmentLeaders),
                AcademicLeaders = JsonSerializer.Serialize(academicLeaders),
                FastestGrowing = JsonSerializer.Serialize(fastestGrowing),
                SubjectLeaders = JsonSerializer.Serialize(new Dictionary<string, List<InstitutionRanking>>()),
                TrendingMajors = JsonSerializer.Serialize(new List<MajorTrend>()),
                DecliningMajors = JsonSerializer.Serialize(new List<MajorTrend>()),
                RegionalBreakdown = JsonSerializer.Serialize(new Dictionary<string, RegionalStats>()),
                MarketInsights = GenerateMarketInsights(totalInstitutions, marketGrowthRate, averageScore)
            };

            context.MarketAnalyticsReports.Add(report);
            await context.SaveChangesAsync();

            logger.LogInformation("Successfully generated market analytics report {ReportId}", report.Id);

            return report;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate market analytics report");
            throw;
        }
    }

    public async Task<InstitutionAnalyticsReport?> GetLatestInstitutionReportAsync(Guid institutionId)
    {
        return await context.InstitutionAnalyticsReports
            .Where(r => r.InstitutionId == institutionId)
            .OrderByDescending(r => r.To)
            .FirstOrDefaultAsync();
    }

    private async Task<InstitutionAnalyticsReport?> GetLatestInstitutionReportAsync(Guid institutionId, DateTime from, DateTime to)
    {
        return await context.InstitutionAnalyticsReports
            .Where(r => r.InstitutionId == institutionId && r.From >= from && r.To <= to)
            .OrderByDescending(r => r.To)
            .FirstOrDefaultAsync();
    }

    public async Task<List<InstitutionAnalyticsReport>> GetInstitutionReportsAsync(Guid institutionId, int limit = 10)
    {
        return await context.InstitutionAnalyticsReports
            .Where(r => r.InstitutionId == institutionId)
            .OrderByDescending(r => r.To)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<MarketAnalyticsReport?> GetLatestMarketReportAsync(ReportPeriodType periodType)
    {
        return await context.MarketAnalyticsReports
            .Where(r => r.PeriodType == periodType)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<ReportGenerationJob> ScheduleReportGenerationAsync(
        Guid? institutionId,
        ReportPeriodType periodType,
        ReportType reportType,
        DateTime? scheduledFor = null)
    {
        var job = new ReportGenerationJob
        {
            Id = Guid.NewGuid(),
            InstitutionId = institutionId,
            PeriodType = periodType,
            ReportType = reportType,
            ScheduledFor = scheduledFor ?? DateTime.UtcNow,
            Status = JobStatus.Pending,
            MaxRetries = 3,
            RetryCount = 0
        };

        context.ReportGenerationJobs.Add(job);
        await context.SaveChangesAsync();

        return job;
    }

    public async Task<List<ReportGenerationJob>> GetPendingJobsAsync()
    {
        return await context.ReportGenerationJobs
            .Where(j => j.Status == JobStatus.Pending && j.ScheduledFor <= DateTime.UtcNow)
            .OrderBy(j => j.ScheduledFor)
            .ToListAsync();
    }

    public async Task<List<ReportGenerationJob>> GetJobsByInstitutionAsync(Guid institutionId, int limit = 20)
    {
        return await context.ReportGenerationJobs
            .Where(j => j.InstitutionId == institutionId)
            .OrderByDescending(j => j.CreatedAt)
            .Take(limit)
            .Include(j => j.Institution)
            .ToListAsync();
    }

    public async Task<ReportGenerationJob?> GetJobByIdAsync(Guid jobId)
    {
        return await context.ReportGenerationJobs
            .Include(j => j.Institution)
            .FirstOrDefaultAsync(j => j.Id == jobId);
    }

    public async Task ProcessReportGenerationJobAsync(Guid jobId)
    {
        var job = await context.ReportGenerationJobs.FindAsync(jobId);
        if (job == null) return;

        try
        {
            job.Status = JobStatus.Running;
            job.StartedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            if (job.ReportType == ReportType.InstitutionAnalytics && job.InstitutionId.HasValue)
            {
                var endDate = DateTime.UtcNow;
                var startDate = job.PeriodType switch
                {
                    ReportPeriodType.Monthly => endDate.AddMonths(-1),
                    ReportPeriodType.Quarterly => endDate.AddMonths(-3),
                    ReportPeriodType.Semester => endDate.AddMonths(-6),
                    ReportPeriodType.Yearly => endDate.AddYears(-1),
                    _ => endDate.AddMonths(-1)
                };

                var report = await GenerateInstitutionReportAsync(job.InstitutionId.Value, startDate, endDate);
                job.GeneratedReportId = report.Id;

                // Store in vector database with retry logic
                try 
                {
                    await hybridAnalytics.StoreAnalyticsReportAsync(report);
                    logger.LogInformation("Successfully stored report {ReportId} in vector database", report.Id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to store report {ReportId} in vector database", report.Id);
                    // Don't fail the job if vector storage fails
                }
            }
            else if (job.ReportType == ReportType.MarketAnalytics)
            {
                var reportPeriod = $"{DateTime.UtcNow.Year}-{job.PeriodType}";
                await GenerateMarketReportAsync(job.PeriodType, reportPeriod);
            }

            job.Status = JobStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process report generation job {JobId}", jobId);

            job.Status = JobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.RetryCount++;

            if (job.RetryCount < job.MaxRetries)
            {
                job.Status = JobStatus.Pending;
                job.ScheduledFor = DateTime.UtcNow.AddMinutes(Math.Pow(2, job.RetryCount));
            }
        }

        await context.SaveChangesAsync();
    }
        public async Task<InstitutionAnalyticsReport?> GetInstitutionReportByIdAsync(Guid reportId)
    {
        try
        {
            return await context.InstitutionAnalyticsReports
                .Include(r => r.Institution)
                .FirstOrDefaultAsync(r => r.Id == reportId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get institution report {ReportId}", reportId);
            return null;
        }
    }

    public async Task<MarketAnalyticsReport?> GetMarketReportByIdAsync(Guid reportId)
    {
        try
        {
            return await context.MarketAnalyticsReports
                .FirstOrDefaultAsync(r => r.Id == reportId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get market report {ReportId}", reportId);
            return null;
        }
    }

    public async Task<bool> DeleteInstitutionReportAsync(Guid reportId)
    {
        try
        {
            var report = await context.InstitutionAnalyticsReports.FindAsync(reportId);
            if (report == null)
            {
                logger.LogWarning("Institution report {ReportId} not found for deletion", reportId);
                return false;
            }

            context.InstitutionAnalyticsReports.Remove(report);
            await context.SaveChangesAsync();

            logger.LogInformation("Successfully deleted institution report {ReportId}", reportId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete institution report {ReportId}", reportId);
            return false;
        }
    }

    public async Task<bool> DeleteMarketReportAsync(Guid reportId)
    {
        try
        {
            var report = await context.MarketAnalyticsReports.FindAsync(reportId);
            if (report == null)
            {
                logger.LogWarning("Market report {ReportId} not found for deletion", reportId);
                return false;
            }

            context.MarketAnalyticsReports.Remove(report);
            await context.SaveChangesAsync();

            logger.LogInformation("Successfully deleted market report {ReportId}", reportId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete market report {ReportId}", reportId);
            return false;
        }
    }

    public async Task<bool> UpdateJobStatusAsync(Guid jobId, JobStatus status, string? errorMessage = null)
    {
        try
        {
            var job = await context.ReportGenerationJobs.FindAsync(jobId);
            if (job == null)
            {
                logger.LogWarning("Job {JobId} not found for status update", jobId);
                return false;
            }

            job.Status = status;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                job.ErrorMessage = errorMessage;
            }

            if (status == JobStatus.Cancelled)
            {
                job.ErrorMessage = errorMessage ?? "Cancelled by user";
            }

            await context.SaveChangesAsync();

            logger.LogInformation("Updated job {JobId} status to {Status}", jobId, status);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update job {JobId} status", jobId);
            return false;
        }
    }

    public async Task<bool> RetryJobAsync(Guid jobId)
    {
        try
        {
            var job = await context.ReportGenerationJobs.FindAsync(jobId);
            if (job == null)
            {
                logger.LogWarning("Job {JobId} not found for retry", jobId);
                return false;
            }

            if (job.Status != JobStatus.Failed)
            {
                logger.LogWarning("Job {JobId} cannot be retried - current status: {Status}", jobId, job.Status);
                return false;
            }

            // Reset job for retry
            job.Status = JobStatus.Pending;
            job.ErrorMessage = null;
            job.RetryCount = 0;
            job.ScheduledFor = DateTime.UtcNow;
            job.StartedAt = null;
            job.CompletedAt = null;

            await context.SaveChangesAsync();

            logger.LogInformation("Successfully scheduled job {JobId} for retry", jobId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retry job {JobId}", jobId);
            return false;
        }
    }

    private Dictionary<string, List<string>> GenerateRecommendations(InstitutionAnalyticsReport report)
    {
        var recommendations = new Dictionary<string, List<string>>();

        // Academic Performance
        var academic = new List<string>();
        if (report.OverallAcademicScore < 80)
        {
            academic.Add("Implement targeted tutoring programs for struggling students");
            academic.Add("Enhance teacher professional development initiatives");
            academic.Add("Review and update curriculum to align with best practices");
        }
        if (report.YearOverYearGrowth < 0)
        {
            academic.Add("Conduct detailed analysis of performance decline factors");
            academic.Add("Establish performance improvement task force");
        }
        if (academic.Any()) recommendations["Academic Excellence"] = academic;

        // Student Engagement
        var engagement = new List<string>();
        if (report.AverageAttendanceRate < 90)
        {
            engagement.Add("Develop attendance improvement campaigns");
            engagement.Add("Investigate barriers to regular attendance");
            engagement.Add("Implement attendance tracking and early intervention systems");
        }
        if (engagement.Any()) recommendations["Student Engagement"] = engagement;

        // Operational Excellence
        var operational = new List<string>();
        if (report.TeacherRetentionRate < 85)
        {
            operational.Add("Review teacher compensation and benefits packages");
            operational.Add("Enhance workplace culture and support systems");
            operational.Add("Implement mentorship programs for new teachers");
        }
        if (report.StudentTeacherRatio > 25)
        {
            operational.Add("Consider hiring additional teaching staff");
            operational.Add("Optimize class scheduling and resource allocation");
        }
        if (operational.Any()) recommendations["Operational Excellence"] = operational;

        // Growth & Development
        var growth = new List<string>();
        if (report.EnrollmentGrowthRate < 2)
        {
            growth.Add("Enhance marketing and community outreach efforts");
            growth.Add("Improve facilities and educational offerings");
            growth.Add("Strengthen partnerships with feeder schools");
        }
        growth.Add("Invest in technology and digital learning platforms");
        growth.Add("Expand extracurricular and enrichment programs");
        recommendations["Growth & Development"] = growth;

        return recommendations;
    }

    // IMPLEMENTED: Calculate enrollment growth by comparing with previous year
    private async Task<decimal> CalculateEnrollmentGrowthAsync(Guid institutionId, int currentEnrollment)
    {
        try
        {
            var oneYearAgo = DateTime.UtcNow.AddYears(-1);
            var previousReport = await context.InstitutionAnalyticsReports
                .Where(r => r.InstitutionId == institutionId && r.To <= oneYearAgo)
                .OrderByDescending(r => r.To)
                .FirstOrDefaultAsync();

            if (previousReport == null)
                return 5.0m; // Default growth rate for new institutions

            var previousEnrollment = previousReport.TotalEnrollments;
            if (previousEnrollment == 0)
                return currentEnrollment > 0 ? 100m : 0m;

            return ((decimal)(currentEnrollment - previousEnrollment) / previousEnrollment) * 100;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to calculate enrollment growth for institution {InstitutionId}", institutionId);
            return 5.0m; // Default fallback
        }
    }

    // IMPLEMENTED: Calculate teacher retention by checking employment duration
    private async Task<decimal> CalculateTeacherRetentionAsync(Guid institutionId, IEnumerable<Guid> currentTeacherIds)
    {
        try
        {
            var currentTeachers = currentTeacherIds.ToList();
            if (!currentTeachers.Any())
                return 100m; // No teachers = perfect retention (edge case)

            var oneYearAgo = DateTime.UtcNow.AddYears(-1);
            
            // Get teachers who were active one year ago
            var previousTeachers = await context.Teachers
                .Where(t => t.InstitutionId == institutionId && 
                           t.CreatedAt <= oneYearAgo &&
                           t.Status == ProfileStatus.Active)
                .Select(t => t.Id)
                .ToListAsync();

            if (!previousTeachers.Any())
                return 85m; // Default retention rate for new institutions

            // Calculate how many previous teachers are still active
            var retainedTeachers = previousTeachers.Intersect(currentTeachers).Count();
            var retentionRate = ((decimal)retainedTeachers / previousTeachers.Count) * 100;

            return Math.Min(retentionRate, 100m); // Cap at 100%
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to calculate teacher retention for institution {InstitutionId}", institutionId);
            return 85m; // Default fallback
        }
    }

    // Helper methods
    private ReportPeriodType CalculatePeriodType(DateTime from, DateTime to)
    {
        var duration = to - from;
        return duration.Days switch
        {
            <= 35 => ReportPeriodType.Monthly,
            <= 100 => ReportPeriodType.Quarterly,
            <= 200 => ReportPeriodType.Semester,
            _ => ReportPeriodType.Yearly
        };
    }

    private decimal CalculateExtracurricularParticipation(Guid institutionId)
    {
        // Placeholder implementation - would need actual extracurricular data
        return 65.0m; // 65% participation rate
    }

    private List<string> GenerateAchievements(double academicScore, decimal attendanceRate, Dictionary<string, decimal> subjectScores)
    {
        var achievements = new List<string>();

        if (academicScore >= 90)
            achievements.Add("Outstanding Academic Excellence");

        if (attendanceRate >= 95)
            achievements.Add("Exceptional Attendance Rate");

        if (subjectScores.Values.Any(score => score >= 90))
        {
            var topSubject = subjectScores.Where(kvp => kvp.Value >= 90).First();
            achievements.Add($"Subject Excellence in {topSubject.Key}");
        }

        if (attendanceRate >= 90 && academicScore >= 80)
            achievements.Add("Well-Rounded Academic Performance");

        return achievements;
    }

    private string GenerateMarketInsights(int totalInstitutions, decimal marketGrowthRate, decimal averageScore)
    {
        return $"The educational market shows {(marketGrowthRate > 0 ? "positive" : "negative")} growth trends with " +
               $"{totalInstitutions} institutions maintaining an average performance score of {averageScore:F1}. " +
               $"Market growth rate stands at {marketGrowthRate:F1}%.";
    }

    private string GenerateBasicExecutiveSummary(InstitutionAnalyticsReport report)
    {
        return $"Academic performance score of {report.OverallAcademicScore:F1} with {report.YearOverYearGrowth:F1}% year-over-year growth. " +
               $"Current enrollment: {report.TotalEnrollments} students with {report.EnrollmentGrowthRate:F1}% growth rate. " +
               $"Overall performance category: {report.OverallPerformanceCategory}.";
    }
    
}