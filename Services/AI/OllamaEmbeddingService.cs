using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Models.Vector;

namespace UniTrackRemaster.Services.AI;

public class OllamaEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaEmbeddingService> _logger;
    private readonly string _embeddingModel;

    public OllamaEmbeddingService(HttpClient httpClient, IConfiguration configuration, ILogger<OllamaEmbeddingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _embeddingModel = configuration.GetSection("OllamaSettings").GetValue<string>("EmbeddingModel");
        _httpClient.BaseAddress = new Uri(configuration.GetSection("OllamaSettings").GetSection("BaseUrl").Value);    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        const int maxRetries = 3;
        const int baseDelayMs = 1000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogDebug("Generating embedding for text of length {Length}, attempt {Attempt}", text.Length, attempt);

                // Truncate text if too long
                if (text.Length > 8000)
                {
                    text = text[..8000] + "...";
                    _logger.LogWarning("Text truncated to 8000 characters for embedding generation");
                }

                var request = new
                {
                    model = _embeddingModel, // FIX: Use the correct model
                    prompt = text
                };

                var response = await _httpClient.PostAsJsonAsync("/api/embeddings", request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Ollama request failed: {response.StatusCode} - {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<OllamaEmbeddingResponse>(content);

                if (result?.Embedding == null || result.Embedding.Length == 0)
                {
                    throw new InvalidOperationException("Ollama returned null or empty embedding");
                }

                var floatEmbedding = result.Embedding.Select(d => (float)d).ToArray();
                _logger.LogDebug("Successfully generated embedding with {Dimensions} dimensions", floatEmbedding.Length);

                return floatEmbedding;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                var delay = baseDelayMs * (int)Math.Pow(2, attempt - 1); // Exponential backoff
                _logger.LogWarning(ex, "Embedding generation failed on attempt {Attempt}/{MaxRetries}. Retrying in {Delay}ms", 
                    attempt, maxRetries, delay);
                
                await Task.Delay(delay);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate embedding after {MaxRetries} attempts", maxRetries);
                throw;
            }
        }

        throw new InvalidOperationException("Should not reach here");
    }

    public async Task<float[]> GenerateEmbeddingAsync(AnalyticsDocument document)
    {
        var combinedText = CreateCombinedText(document);
        return await GenerateEmbeddingAsync(combinedText);
    }

    private static string CreateCombinedText(AnalyticsDocument document)
    {
        return $"""
            Institution: {document.InstitutionName}
            Type: {document.Metadata.InstitutionType}
            Region: {document.Metadata.Region}
            
            Summary: {document.Content.ExecutiveSummary}
            
            Achievements: {string.Join(", ", document.Content.KeyAchievements)}
            
            Analysis: {document.Content.PerformanceAnalysis}
            
            Areas for Improvement: {string.Join(", ", document.Content.ImprovementAreas)}
            
            Recommendations: {string.Join(", ", document.Content.Recommendations)}
            """;
    }

    private record OllamaEmbeddingResponse([property: JsonPropertyName("embedding")] double[] Embedding);
}