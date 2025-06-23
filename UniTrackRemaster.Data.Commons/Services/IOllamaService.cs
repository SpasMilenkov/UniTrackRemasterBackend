using UniTrackRemaster.Data.Models.Vector;

namespace UniTrackRemaster.Commons.Services;

public interface IOllamaService
{
    Task<float[]> GenerateEmbeddingAsync(string text);
    Task<string> GenerateChatResponseAsync(string prompt, string? systemPrompt = null);
    Task<string> GenerateInsightAsync(string question, List<string> context);
    Task<List<string>> GenerateRecommendationsAsync(AnalyticsDocument document, List<AnalyticsDocument> similarInstitutions);
}