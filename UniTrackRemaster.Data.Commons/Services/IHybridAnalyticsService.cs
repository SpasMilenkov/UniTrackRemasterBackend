using UniTrackRemaster.Api.Dto.Analytics;
using UniTrackRemaster.Data.Models.Analytics;
using UniTrackRemaster.Data.Models.Vector;

namespace UniTrackRemaster.Commons.Services;

public interface IHybridAnalyticsService
{
    // Initialization
    Task InitializeCollectionAsync();

    // Document storage and retrieval
    Task<AnalyticsDocument> StoreAnalyticsReportAsync(InstitutionAnalyticsReport sqlReport);
    Task<AnalyticsDocument?> GetAnalyticsDocumentAsync(Guid institutionId, string? reportPeriod = null);

    // Vector search operations
    Task<List<AnalyticsDocument>> SearchSimilarInstitutionsAsync(Guid institutionId, int limit = 5);
    Task<List<AnalyticsDocument>> SearchByQueryAsync(string query, int limit = 10);

    // AI-powered insights
    Task<string> GenerateInsightAsync(string question);
    Task<string> AskQuestionAsync(string question, Guid? institutionId = null);
    Task<List<string>> GetAIRecommendationsAsync(Guid institutionId);
    Task<string> GenerateExecutiveSummaryAsync(InstitutionAnalyticsReport report);

    // Comparison features
    Task<InstitutionComparisonDto> CompareInstitutionsAsync(Guid institutionId1, Guid institutionId2);
}