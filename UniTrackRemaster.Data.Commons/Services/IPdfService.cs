using UniTrackRemaster.Data.Models.Analytics;

namespace UniTrackRemaster.Commons.Services;

public interface IPdfService
{
    Task<byte[]> GenerateInstitutionAnalyticsReportAsync(InstitutionAnalyticsReport report);
    Task<byte[]> GenerateMarketAnalyticsReportAsync(MarketAnalyticsReport report);
    Task<byte[]> GenerateComparisonReportAsync(InstitutionAnalyticsReport report1, InstitutionAnalyticsReport report2);
}