using UniTrackRemaster.Data.Models.Analytics;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Commons.Services;

public interface IAnalyticsReportService
{
    // Existing methods
    Task<InstitutionAnalyticsReport> GenerateInstitutionReportAsync(Guid institutionId, DateTime from, DateTime to);
    Task<MarketAnalyticsReport> GenerateMarketReportAsync(ReportPeriodType periodType, string reportPeriod);
    Task<InstitutionAnalyticsReport?> GetLatestInstitutionReportAsync(Guid institutionId);
    Task<List<InstitutionAnalyticsReport>> GetInstitutionReportsAsync(Guid institutionId, int limit = 10);
    Task<MarketAnalyticsReport?> GetLatestMarketReportAsync(ReportPeriodType periodType);
    Task<ReportGenerationJob> ScheduleReportGenerationAsync(Guid? institutionId, ReportPeriodType periodType, ReportType reportType, DateTime? scheduledFor = null);
    Task<List<ReportGenerationJob>> GetPendingJobsAsync();
    Task<List<ReportGenerationJob>> GetJobsByInstitutionAsync(Guid institutionId, int limit = 20);
    Task<ReportGenerationJob?> GetJobByIdAsync(Guid jobId);
    Task ProcessReportGenerationJobAsync(Guid jobId);

    // New methods for controller operations
    Task<InstitutionAnalyticsReport?> GetInstitutionReportByIdAsync(Guid reportId);
    Task<MarketAnalyticsReport?> GetMarketReportByIdAsync(Guid reportId);
    Task<bool> DeleteInstitutionReportAsync(Guid reportId);
    Task<bool> DeleteMarketReportAsync(Guid reportId);
    Task<bool> UpdateJobStatusAsync(Guid jobId, JobStatus status, string? errorMessage = null);
    Task<bool> RetryJobAsync(Guid jobId);
}