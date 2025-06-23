using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Models.Vector;

namespace UniTrackRemaster.Services.AI;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaService> _logger;
    private readonly string _embeddingModel;
    private readonly string _chatModel;
    private readonly double _temperature;
    private readonly int _maxTokens;

    public OllamaService(HttpClient httpClient, IConfiguration configuration, ILogger<OllamaService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Get configuration values directly
        var baseUrl = configuration.GetValue<string>("OllamaSettings:BaseUrl") ?? "http://localhost:11434";
        _embeddingModel = configuration.GetValue<string>("OllamaSettings:EmbeddingModel") ?? "nomic-embed-text";
        _chatModel = configuration.GetValue<string>("OllamaSettings:ChatModel") ?? "llama3.2:3b";
        _temperature = configuration.GetValue("OllamaSettings:Temperature", 0.7);
        _maxTokens = configuration.GetValue("OllamaSettings:MaxTokens", 2048);

        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromMinutes(5); // Increase timeout for embeddings
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        try
        {
            _logger.LogDebug("Generating embedding for text of length {Length}", text.Length);

            // Truncate text if too long (Ollama has limits)
            if (text.Length > 8000)
            {
                text = text[..8000] + "...";
                _logger.LogWarning("Text truncated to 8000 characters for embedding generation");
            }

            var request = new
            {
                model = _embeddingModel,
                prompt = text
            };

            _logger.LogDebug("Sending embedding request to Ollama with model {Model}", _embeddingModel);    

            var response = await _httpClient.PostAsJsonAsync("/api/embeddings", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Ollama embedding request failed with status {StatusCode}: {Error}", 
                    response.StatusCode, errorContent);
                throw new InvalidOperationException($"Ollama request failed: {response.StatusCode} - {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Received embedding response of length {Length}", content.Length);

            var result = JsonSerializer.Deserialize<OllamaEmbeddingResponse>(content);

            if (result?.Embedding == null || result.Embedding.Length == 0)
            {
                _logger.LogError("Ollama returned null or empty embedding. Response: {Response}", content);
                throw new InvalidOperationException("Ollama returned null or empty embedding");
            }

            var floatEmbedding = result.Embedding.Select(d => (float)d).ToArray();
            _logger.LogDebug("Successfully generated embedding with {Dimensions} dimensions", floatEmbedding.Length);

            return floatEmbedding;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while connecting to Ollama at {BaseUrl}", _httpClient.BaseAddress);
            throw new InvalidOperationException($"Failed to connect to Ollama: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while generating embedding (text length: {Length})", text.Length);
            throw new InvalidOperationException($"Ollama request timed out: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Ollama embedding response");
            throw new InvalidOperationException($"Invalid response from Ollama: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating embedding");
            throw new InvalidOperationException($"Failed to generate embedding: {ex.Message}", ex);
        }
    }

    public async Task<string> GenerateChatResponseAsync(string prompt, string? systemPrompt = null)
    {
        try
        {
            _logger.LogDebug("Generating chat response for prompt of length {Length}", prompt.Length);

            var fullPrompt = systemPrompt != null ? $"{systemPrompt}\n\nUser: {prompt}" : prompt;

            var request = new
            {
                model = _chatModel,
                prompt = fullPrompt,
                stream = false,
                options = new
                {
                    temperature = _temperature,
                    num_predict = _maxTokens
                }
            };

            var response = await _httpClient.PostAsJsonAsync("/api/generate", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Ollama chat request failed with status {StatusCode}: {Error}", 
                    response.StatusCode, errorContent);
                throw new InvalidOperationException($"Ollama request failed: {response.StatusCode} - {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OllamaChatResponse>(content);

            if (string.IsNullOrEmpty(result?.Response))
            {
                _logger.LogError("Ollama returned empty chat response. Response: {Response}", content);
                throw new InvalidOperationException("Ollama returned empty chat response");
            }

            _logger.LogDebug("Successfully generated chat response of length {Length}", result.Response.Length);
            return result.Response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while connecting to Ollama for chat");
            throw new InvalidOperationException($"Failed to connect to Ollama: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while generating chat response");
            throw new InvalidOperationException($"Ollama request timed out: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Ollama chat response");
            throw new InvalidOperationException($"Invalid response from Ollama: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating chat response");
            throw new InvalidOperationException($"Failed to generate chat response: {ex.Message}", ex);
        }
    }

    public async Task<string> GenerateInsightAsync(string question, List<string> context)
    {
        var contextText = string.Join("\n\n", context.Select((ctx, i) => $"Context {i + 1}:\n{ctx}"));

        var systemPrompt = """
                           You are an expert educational data analyst. Based on the provided context from educational institution reports, 
                           answer the user's question with specific insights and actionable recommendations.

                           Guidelines:
                           - Use specific data points from the context when possible
                           - Provide actionable insights
                           - Compare and contrast institutions when relevant
                           - Keep responses concise but informative
                           - If the context doesn't contain enough information, say so clearly
                           """;

        var prompt = $"""
                      Context Information:
                      {contextText}

                      Question: {question}

                      Please provide a detailed analysis based on the context above.
                      """;

        return await GenerateChatResponseAsync(prompt, systemPrompt);
    }

    public async Task<List<string>> GenerateRecommendationsAsync(AnalyticsDocument document,
        List<AnalyticsDocument> similarInstitutions)
    {
        var similarInstitutionsText = string.Join("\n\n", similarInstitutions.Select(inst =>
            $"Institution: {inst.InstitutionName}\n" +
            $"Score: {inst.Metadata.OverallScore}\n" +
            $"Achievements: {string.Join(", ", inst.Content.KeyAchievements)}\n" +
            $"Recommendations: {string.Join(", ", inst.Content.Recommendations)}"));

        var systemPrompt = """
                           You are an educational consultant specializing in institutional improvement. 
                           Analyze the current institution's performance and compare it with similar high-performing institutions 
                           to generate specific, actionable recommendations for improvement.

                           Focus on:
                           - Areas where similar institutions are outperforming
                           - Specific strategies that successful institutions are using
                           - Practical next steps that can be implemented
                           - Measurable outcomes to track progress
                           """;

        var prompt = $"""
                      Current Institution Analysis:
                      Institution: {document.InstitutionName}
                      Overall Score: {document.Metadata.OverallScore}
                      Student Count: {document.Metadata.StudentCount}
                      Current Achievements: {string.Join(", ", document.Content.KeyAchievements)}
                      Current Improvement Areas: {string.Join(", ", document.Content.ImprovementAreas)}

                      Similar High-Performing Institutions:
                      {similarInstitutionsText}

                      Generate 5-7 specific, actionable recommendations for improvement based on successful strategies 
                      from similar institutions. Format each recommendation as a clear action item.
                      """;

        var response = await GenerateChatResponseAsync(prompt, systemPrompt);

        // Parse the response into a list of recommendations
        return response.Split('\n')
            .Where(line =>
                line.Trim().Length > 0 &&
                (line.StartsWith('-') || line.StartsWith('•') || char.IsDigit(line.Trim()[0])))
            .Select(line => line.Trim().TrimStart('-', '•', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.', ' '))
            .Where(rec => rec.Length > 10) // Filter out very short lines
            .ToList();
    }

    // Response DTOs
    private record OllamaEmbeddingResponse([property: JsonPropertyName("embedding")] double[] Embedding);
    private record OllamaChatResponse(   [property: JsonPropertyName("response")] string Response, 
        [property: JsonPropertyName("done")] bool Done);
}