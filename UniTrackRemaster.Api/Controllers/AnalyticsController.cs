using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using UniTrackRemaster.Api.Dto.Analytics;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Analytics;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Vector;

namespace UniTrackRemaster.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IHybridAnalyticsService _hybridAnalytics;
    private readonly IAnalyticsReportService _reportService;
    private readonly IPdfService _pdfService;
    private readonly ILogger<AnalyticsController> _logger;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _shortCacheExpiration = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _longCacheExpiration = TimeSpan.FromMinutes(30);

    public AnalyticsController(
        IHybridAnalyticsService hybridAnalytics,
        IAnalyticsReportService reportService,
        IPdfService pdfService,
        ILogger<AnalyticsController> logger,
        IMemoryCache cache)
    {
        _hybridAnalytics = hybridAnalytics;
        _reportService = reportService;
        _pdfService = pdfService;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    ///     Get analytics dashboard for an institution
    /// </summary>
    [HttpGet("dashboard/{institutionId}")]
    [ResponseCache(Duration = 300)] // 5 minutes
    public async Task<ActionResult<AnalyticsDashboardDto>> GetDashboard(Guid institutionId)
    {
        var cacheKey = $"dashboard_{institutionId}";

        // Check cache first
        if (_cache.TryGetValue(cacheKey, out AnalyticsDashboardDto? cachedDashboard)) return Ok(cachedDashboard);

        try
        {
            _logger.LogInformation("Generating dashboard for institution {InstitutionId}", institutionId);

            var report = await _reportService.GetLatestInstitutionReportAsync(institutionId);
            if (report == null) return NotFound("No analytics report found for this institution");

            // Get AI-powered data in parallel for better performance
            var analyticsDocTask = _hybridAnalytics.GetAnalyticsDocumentAsync(institutionId);
            var recommendationsTask = _hybridAnalytics.GetAIRecommendationsAsync(institutionId);
            var similarInstitutionsTask = _hybridAnalytics.SearchSimilarInstitutionsAsync(institutionId);

            await Task.WhenAll(analyticsDocTask, recommendationsTask, similarInstitutionsTask);

            var analyticsDoc = await analyticsDocTask;
            var recommendations = await recommendationsTask;
            var similarInstitutions = await similarInstitutionsTask;

            var dashboard = new AnalyticsDashboardDto(
                new InstitutionOverviewDto(
                    report.InstitutionId,
                    report.Institution?.Name ?? "Unknown",
                    report.Institution?.Type.ToString() ?? "Unknown",
                    report.TotalEnrollments,
                    report.OverallAcademicScore,
                    report.OverallPerformanceCategory.ToString(),
                    null, // National rank would come from rankings
                    null // Regional rank would come from rankings
                ),
                new PerformanceMetricsDto(
                    report.OverallAcademicScore,
                    report.YearOverYearGrowth,
                    report.AverageAttendanceRate,
                    report.TeacherRetentionRate,
                    report.StudentTeacherRatio,
                    JsonSerializer.Deserialize<Dictionary<string, decimal>>(report.SubjectPerformanceScores) ??
                    new Dictionary<string, decimal>()
                ),
                JsonSerializer.Deserialize<List<string>>(report.TopAchievements)?
                    .Select(a => new AchievementDto(a, a, "Academic", report.CreatedAt))
                    .ToList() ?? new List<AchievementDto>(),
                GenerateTrendsFromReport(report), // Generate basic trends
                new ComparisonDataDto(
                    similarInstitutions.Select(doc => new PeerComparisonDto(
                        doc.InstitutionId,
                        doc.InstitutionName,
                        doc.Metadata.OverallScore,
                        100m, // Similarity score placeholder
                        doc.Metadata.InstitutionType,
                        new Dictionary<string, decimal>()
                    )).ToList(),
                    new BenchmarkDataDto(75m, 78m, 80m, new Dictionary<string, decimal>()),
                    new RankingDataDto(null, null, null, 100, new Dictionary<string, int>())
                ),
                recommendations.Select(r => new RecommendationDto(
                    r, r, "Improvement", Priority.Medium, new List<string> { r }
                )).ToList(),
                report.CreatedAt
            );

            // Cache the dashboard
            _cache.Set(cacheKey, dashboard, _shortCacheExpiration);

            _logger.LogInformation("Successfully generated dashboard for institution {InstitutionId}", institutionId);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get dashboard for institution {InstitutionId}", institutionId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Generate a new analytics report (async)
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<ReportJobDto>> GenerateReport([FromBody] GenerateReportRequestDto request)
    {
        try
        {
            _logger.LogInformation("Scheduling report generation for institution {InstitutionId}",
                request.InstitutionId);

            var job = await _reportService.ScheduleReportGenerationAsync(
                request.InstitutionId,
                request.PeriodType,
                ReportType.InstitutionAnalytics
            );

            var jobDto = new ReportJobDto(
                job.Id,
                job.InstitutionId,
                "Institution Name",
                job.ReportType,
                job.PeriodType,
                job.Status,
                job.ScheduledFor,
                job.StartedAt,
                job.CompletedAt,
                job.GeneratedReportId,
                job.ErrorMessage
            );

            _logger.LogInformation("Scheduled report generation job {JobId}", job.Id);
            return Ok(jobDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule report generation");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Chat with AI about analytics data
    /// </summary>
    [HttpPost("chat")]
    public async Task<ActionResult<ChatResponseDto>> Chat([FromBody] ChatQueryDto query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query.Question)) return BadRequest("Question cannot be empty");

            _logger.LogInformation("Processing analytics chat query: {Question}", query.Question);

            var answer = await _hybridAnalytics.AskQuestionAsync(query.Question, query.InstitutionId);

            var response = new ChatResponseDto(
                answer,
                new List<string>(), // Would track which documents were used
                0.85m, // Would come from AI model confidence
                DateTime.UtcNow,
                "Try asking about specific subjects or comparing with peer institutions"
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process chat query: {Question}", query.Question);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Compare two institutions
    /// </summary>
    [HttpPost("compare")]
    [ResponseCache(Duration = 1800)] // 30 minutes
    public async Task<ActionResult<InstitutionComparisonDto>> Compare([FromBody] ComparisonRequestDto request)
    {
        try
        {
            _logger.LogInformation("Comparing institutions {Id1} and {Id2}", request.Institution1Id,
                request.Institution2Id);

            var comparison = await _hybridAnalytics.CompareInstitutionsAsync(
                request.Institution1Id,
                request.Institution2Id
            );

            return Ok(comparison);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compare institutions");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Get market analytics
    /// </summary>
    [HttpGet("market")]
    [ResponseCache(Duration = 3600)] // 1 hour
    public async Task<ActionResult<MarketAnalyticsDto>> GetMarketAnalytics(
        [FromQuery] ReportPeriodType periodType = ReportPeriodType.Quarterly)
    {
        var cacheKey = $"market_analytics_{periodType}";

        if (_cache.TryGetValue(cacheKey, out MarketAnalyticsDto? cachedMarket)) return Ok(cachedMarket);

        try
        {
            var report = await _reportService.GetLatestMarketReportAsync(periodType);
            if (report == null) return NotFound("No market report found for the specified period");

            var marketDto = new MarketAnalyticsDto(
                report.ReportPeriod,
                new MarketOverviewDto(
                    report.TotalInstitutions,
                    report.TotalStudents,
                    report.AverageInstitutionScore,
                    report.MarketGrowthRate,
                    new Dictionary<string, int>() // Would come from report data
                ),
                JsonSerializer.Deserialize<List<UniTrackRemaster.Data.Models.Vector.InstitutionRanking>>(report.AcademicLeaders)?
                    .Select(r => new InstitutionRankingDto(r.InstitutionId, r.InstitutionName, "Unknown", r.Score,
                        r.Rank, r.ChangeFromPrevious))
                    .ToList() ?? new List<InstitutionRankingDto>(),
                JsonSerializer.Deserialize<List<UniTrackRemaster.Data.Models.Vector.MajorTrend>>(report.TrendingMajors)?
                    .Select(m => new MajorTrendDto(m.MajorName, m.CurrentEnrollment, m.GrowthRate, m.Trend,
                        m.InstitutionsOffering, m.AveragePerformanceScore))
                    .ToList() ?? new List<MajorTrendDto>(),
                new List<RegionalStatsDto>(), // Would come from regional breakdown
                report.MarketInsights ?? "No insights available",
                report.CreatedAt
            );

            // Cache market analytics
            _cache.Set(cacheKey, marketDto, _longCacheExpiration);

            return Ok(marketDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get market analytics");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Search similar institutions
    /// </summary>
    [HttpGet("similar/{institutionId}")]
    [ResponseCache(Duration = 900)] // 15 minutes
    public async Task<ActionResult<List<PeerComparisonDto>>> GetSimilarInstitutions(
        Guid institutionId,
        [FromQuery] int limit = 5)
    {
        var cacheKey = $"similar_{institutionId}_{limit}";

        if (_cache.TryGetValue(cacheKey, out List<PeerComparisonDto>? cachedSimilar)) return Ok(cachedSimilar);

        try
        {
            var similarDocs = await _hybridAnalytics.SearchSimilarInstitutionsAsync(institutionId, limit);

            var peers = similarDocs.Select(doc => new PeerComparisonDto(
                doc.InstitutionId,
                doc.InstitutionName,
                doc.Metadata.OverallScore,
                100m, // Similarity percentage would come from vector scores
                doc.Metadata.InstitutionType,
                new Dictionary<string, decimal>()
            )).ToList();

            // Cache similar institutions
            _cache.Set(cacheKey, peers, _shortCacheExpiration);

            return Ok(peers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get similar institutions for {InstitutionId}", institutionId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Get AI recommendations
    /// </summary>
    [HttpGet("recommendations/{institutionId}")]
    [ResponseCache(Duration = 1800)] // 30 minutes
    public async Task<ActionResult<List<RecommendationDto>>> GetRecommendations(Guid institutionId)
    {
        try
        {
            var recommendations = await _hybridAnalytics.GetAIRecommendationsAsync(institutionId);

            var recommendationDtos = recommendations.Select(r => new RecommendationDto(
                r,
                r,
                "Improvement",
                Priority.Medium,
                new List<string> { r }
            )).ToList();

            return Ok(recommendationDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recommendations for {InstitutionId}", institutionId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Get job status by ID
    /// </summary>
    [HttpGet("jobs/{jobId}")]
    public async Task<ActionResult<ReportJobDto>> GetJobStatus(Guid jobId)
    {
        try
        {
            var job = await _reportService.GetJobByIdAsync(jobId);
            if (job == null) return NotFound("Job not found");

            var jobDto = new ReportJobDto(
                job.Id,
                job.InstitutionId,
                job.Institution?.Name ?? "Unknown Institution",
                job.ReportType,
                job.PeriodType,
                job.Status,
                job.ScheduledFor,
                job.StartedAt,
                job.CompletedAt,
                job.GeneratedReportId,
                job.ErrorMessage
            );

            return Ok(jobDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get job status for {JobId}", jobId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Get all jobs for a specific institution
    /// </summary>
    [HttpGet("institutions/{institutionId}/jobs")]
    [ResponseCache(Duration = 60)] // 1 minute cache
    public async Task<ActionResult<List<ReportJobDto>>> GetInstitutionJobs(
        Guid institutionId,
        [FromQuery] int limit = 20)
    {
        try
        {
            _logger.LogInformation("Getting jobs for institution {InstitutionId}", institutionId);

            var jobs = await _reportService.GetJobsByInstitutionAsync(institutionId, limit);

            var jobDtos = jobs.Select(job => new ReportJobDto(
                job.Id,
                job.InstitutionId,
                job.Institution?.Name ?? "Unknown Institution",
                job.ReportType,
                job.PeriodType,
                job.Status,
                job.ScheduledFor,
                job.StartedAt,
                job.CompletedAt,
                job.GeneratedReportId,
                job.ErrorMessage
            )).ToList();

            return Ok(jobDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get jobs for institution {InstitutionId}", institutionId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Get all pending jobs (admin endpoint)
    /// </summary>
    [HttpGet("jobs/pending")]
    public async Task<ActionResult<List<ReportJobDto>>> GetPendingJobs()
    {
        try
        {
            var jobs = await _reportService.GetPendingJobsAsync();

            var jobDtos = jobs.Select(job => new ReportJobDto(
                job.Id,
                job.InstitutionId,
                job.Institution?.Name ?? "Unknown Institution",
                job.ReportType,
                job.PeriodType,
                job.Status,
                job.ScheduledFor,
                job.StartedAt,
                job.CompletedAt,
                job.GeneratedReportId,
                job.ErrorMessage
            )).ToList();

            return Ok(jobDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get pending jobs");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Download a completed report as PDF
    /// </summary>
    [HttpGet("reports/{reportId}/download")]
    public async Task<ActionResult> DownloadReport(Guid reportId)
    {
        try
        {
            var report = await _reportService.GetInstitutionReportByIdAsync(reportId);
            if (report == null) 
                return NotFound("Report not found");

            _logger.LogInformation("Generating PDF download for report {ReportId}", reportId);

            var pdfContent = await _pdfService.GenerateInstitutionAnalyticsReportAsync(report);

            var fileName = $"{SanitizeFileName(report.Institution?.Name ?? "Report")}_{report.PeriodType}_{report.CreatedAt:yyyy-MM-dd}.pdf";

            _logger.LogInformation("Successfully generated PDF for report {ReportId}, size: {Size} KB", 
                reportId, pdfContent.Length / 1024);

            return File(pdfContent, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download report {ReportId}", reportId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Download market analytics report as PDF
    /// </summary>
    [HttpGet("market/{reportId}/download")]
    public async Task<ActionResult> DownloadMarketReport(Guid reportId)
    {
        try
        {
            var report = await _reportService.GetMarketReportByIdAsync(reportId);
            if (report == null) 
                return NotFound("Market report not found");

            _logger.LogInformation("Generating market PDF download for report {ReportId}", reportId);

            var pdfContent = await _pdfService.GenerateMarketAnalyticsReportAsync(report);

            var fileName = $"Market_Analytics_{report.ReportPeriod}_{report.CreatedAt:yyyy-MM-dd}.pdf";

            return File(pdfContent, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download market report {ReportId}", reportId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Download comparison report as PDF
    /// </summary>
    [HttpPost("compare/download")]
    public async Task<ActionResult> DownloadComparisonReport([FromBody] ComparisonRequestDto request)
    {
        try
        {
            var report1 = await _reportService.GetLatestInstitutionReportAsync(request.Institution1Id);
            var report2 = await _reportService.GetLatestInstitutionReportAsync(request.Institution2Id);

            if (report1 == null || report2 == null)
                return NotFound("One or both institution reports not found");

            _logger.LogInformation("Generating comparison PDF for institutions {Id1} vs {Id2}", 
                request.Institution1Id, request.Institution2Id);

            var pdfContent = await _pdfService.GenerateComparisonReportAsync(report1, report2);

            var fileName = $"Comparison_Report_{DateTime.Now:yyyy-MM-dd}.pdf";

            return File(pdfContent, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate comparison PDF");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Delete a report
    /// </summary>
    [HttpDelete("reports/{reportId}")]
    public async Task<ActionResult> DeleteReport(Guid reportId)
    {
        try
        {
            var deleted = await _reportService.DeleteInstitutionReportAsync(reportId);
            if (!deleted)
                return NotFound("Report not found");

            _logger.LogInformation("Report {ReportId} deleted", reportId);
            return Ok(new { message = "Report deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete report {ReportId}", reportId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Delete a market report
    /// </summary>
    [HttpDelete("market/{reportId}")]
    public async Task<ActionResult> DeleteMarketReport(Guid reportId)
    {
        try
        {
            var deleted = await _reportService.DeleteMarketReportAsync(reportId);
            if (!deleted)
                return NotFound("Market report not found");

            _logger.LogInformation("Market report {ReportId} deleted", reportId);
            return Ok(new { message = "Market report deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete market report {ReportId}", reportId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Cancel a report generation job
    /// </summary>
    [HttpPost("jobs/{jobId}/cancel")]
    public async Task<ActionResult> CancelJob(Guid jobId)
    {
        try
        {
            var job = await _reportService.GetJobByIdAsync(jobId);
            if (job == null) 
                return NotFound("Job not found");

            if (job.Status != JobStatus.Pending && job.Status != JobStatus.Running)
                return BadRequest("Job cannot be cancelled in its current state");

            var updated = await _reportService.UpdateJobStatusAsync(jobId, JobStatus.Cancelled, "Cancelled by user");
            if (!updated)
                return StatusCode(500, "Failed to cancel job");

            _logger.LogInformation("Job {JobId} cancelled by user", jobId);
            return Ok(new { message = "Job cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel job {JobId}", jobId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Retry a failed report generation job
    /// </summary>
    [HttpPost("jobs/{jobId}/retry")]
    public async Task<ActionResult> RetryJob(Guid jobId)
    {
        try
        {
            var job = await _reportService.GetJobByIdAsync(jobId);
            if (job == null) 
                return NotFound("Job not found");

            if (job.Status != JobStatus.Failed) 
                return BadRequest("Only failed jobs can be retried");

            var retried = await _reportService.RetryJobAsync(jobId);
            if (!retried)
                return StatusCode(500, "Failed to retry job");

            _logger.LogInformation("Job {JobId} scheduled for retry", jobId);
            return Ok(new { message = "Job scheduled for retry" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retry job {JobId}", jobId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Test PDF generation with sample data (remove in production)
    /// </summary>
    [HttpGet("test/pdf")]
    public async Task<ActionResult> TestPdfGeneration()
    {
        try
        {
            // Create sample report data for testing
            var testReport = new Data.Models.Analytics.InstitutionAnalyticsReport
            {
                Id = Guid.NewGuid(),
                InstitutionId = Guid.NewGuid(),
                From = DateTime.Now.AddMonths(-3),
                To = DateTime.Now,
                PeriodType = ReportPeriodType.Quarterly,
                OverallAcademicScore = 85.7m,
                YearOverYearGrowth = 3.2m,
                TotalEnrollments = 1250,
                EnrollmentGrowthRate = 5.8m,
                AverageAttendanceRate = 92.4m,
                StudentTeacherRatio = 18.5m,
                TeacherRetentionRate = 87.3m,
                OverallPerformanceCategory = PerformanceCategory.Good,
                SubjectPerformanceScores = JsonSerializer.Serialize(new Dictionary<string, decimal>
                {
                    ["Mathematics"] = 88.5m,
                    ["English"] = 82.1m,
                    ["Science"] = 91.2m,
                    ["History"] = 79.8m,
                    ["Art"] = 95.3m
                }),
                TopAchievements = JsonSerializer.Serialize(new List<string>
                {
                    "Achieved 95% student satisfaction rating",
                    "Implemented innovative STEM program",
                    "Received state recognition for academic improvement",
                    "Successful technology integration initiative"
                }),
                ExecutiveSummary = "This quarterly report demonstrates strong institutional performance with notable improvements in academic outcomes and student engagement. The institution has successfully maintained high standards while implementing strategic initiatives for continuous improvement.",
                AIGeneratedInsights = "Based on performance data analysis, the institution shows particularly strong performance in STEM subjects and demonstrates effective teaching methodologies. Recommended focus areas include enhancing humanities programs and maintaining the current trajectory in science education.",
                NationalRankings = JsonSerializer.Serialize(new Dictionary<string, int>()),
                RegionalRankings = JsonSerializer.Serialize(new Dictionary<string, int>()),
                DepartmentRankings = JsonSerializer.Serialize(new Dictionary<string, int>()),
                PopularMajors = JsonSerializer.Serialize(new Dictionary<string, int>()),
                MajorGrowthRates = JsonSerializer.Serialize(new Dictionary<string, decimal>()),
                FastestGrowingAreas = JsonSerializer.Serialize(new List<string>()),
                StrongestSubjects = JsonSerializer.Serialize(new List<string> { "Science", "Art", "Mathematics" }),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Generate PDF
            var pdfBytes = await _pdfService.GenerateInstitutionAnalyticsReportAsync(testReport);

            _logger.LogInformation("Test PDF generated successfully, size: {Size} KB", pdfBytes.Length / 1024);

            return File(pdfBytes, "application/pdf", $"Test_Analytics_Report_{DateTime.Now:yyyy-MM-dd_HHmm}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate test PDF");
            return StatusCode(500, $"PDF generation failed: {ex.Message}");
        }
    }

    // Helper method to generate trends from report data
    private List<TrendDto> GenerateTrendsFromReport(Data.Models.Analytics.InstitutionAnalyticsReport report)
    {
        var trends = new List<TrendDto>();

        // Academic Performance Trend
        trends.Add(new TrendDto(
            "Academic Performance",
            "Academic",
            report.OverallAcademicScore,
            report.OverallAcademicScore - report.YearOverYearGrowth,
            report.YearOverYearGrowth,
            report.YearOverYearGrowth > 0 ? TrendDirection.Increasing : TrendDirection.Decreasing,
            new List<TrendDataPointDto>
            {
                new(DateTime.Now.AddMonths(-6), report.OverallAcademicScore - report.YearOverYearGrowth),
                new(DateTime.Now, report.OverallAcademicScore)
            }
        ));

        // Attendance Trend
        trends.Add(new TrendDto(
            "Attendance Rate",
            "Engagement",
            report.AverageAttendanceRate,
            report.AverageAttendanceRate - 2, // Placeholder calculation
            2m, // Placeholder
            TrendDirection.Stable,
            new List<TrendDataPointDto>
            {
                new(DateTime.Now.AddMonths(-6), report.AverageAttendanceRate - 2),
                new(DateTime.Now, report.AverageAttendanceRate)
            }
        ));

        return trends;
    }

    // Helper method to sanitize filenames
    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Where(ch => !invalidChars.Contains(ch)).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "Report" : sanitized;
    }
}