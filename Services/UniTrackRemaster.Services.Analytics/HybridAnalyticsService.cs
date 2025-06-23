using System.Text.Json;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Analytics;
using UniTrackRemaster.Data.Models.Vector;
using UniTrackRemaster.Api.Dto.Analytics;
using Microsoft.EntityFrameworkCore;
using Qdrant.Client.Grpc;
using Condition = Qdrant.Client.Grpc.Condition;
using Match = Qdrant.Client.Grpc.Match;
using UniTrackRemaster.Commons.Services;
using Microsoft.Extensions.Caching.Memory;

namespace UniTrackRemaster.Services.Analytics;

public class HybridAnalyticsService : IHybridAnalyticsService
{
    private readonly IQdrantService _qdrantService;
    private readonly IOllamaService _ollama;
    private readonly IEmbeddingService _embeddingService;
    private readonly UniTrackDbContext _context;
    private readonly ILogger<HybridAnalyticsService> _logger;
    private readonly IMemoryCache _cache;
    private const string COLLECTION_NAME = "analytics_reports";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public HybridAnalyticsService(
        IQdrantService qdrantService,
        IOllamaService ollama,
        IEmbeddingService embeddingService,
        UniTrackDbContext context,
        ILogger<HybridAnalyticsService> logger,
        IMemoryCache cache)
    {
        _qdrantService = qdrantService;
        _ollama = ollama;
        _embeddingService = embeddingService;
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task InitializeCollectionAsync()
    {
        try
        {
            var collectionExists = await _qdrantService.CollectionExistsAsync(COLLECTION_NAME);
            if (!collectionExists)
            {
                await _qdrantService.CreateCollectionAsync(COLLECTION_NAME, new VectorParams
                {
                    Size = 768, // nomic-embed-text dimensions
                    Distance = Distance.Cosine
                });
                _logger.LogInformation("Created Qdrant collection: {CollectionName}", COLLECTION_NAME);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Qdrant collection");
            throw;
        }
    }

    public async Task<AnalyticsDocument> StoreAnalyticsReportAsync(InstitutionAnalyticsReport sqlReport)
    {
        const int maxRetries = 3;
        const int baseDelayMs = 1000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogDebug("Storing analytics document for institution {InstitutionId}, attempt {Attempt}", 
                    sqlReport.InstitutionId, attempt);

                // 1. Create analytics document from SQL report
                var document = await CreateAnalyticsDocumentAsync(sqlReport);

                // 2. Generate embedding using the cached service
                var combinedText = CreateCombinedText(document);
                var embedding = await _embeddingService.GenerateEmbeddingAsync(combinedText);

                // 3. Store in Qdrant using your service
                var point = new PointStruct
                {
                    Id = new PointId { Uuid = document.Id.ToString() },
                    Vectors = embedding
                };

                // Add metadata to payload
                point.Payload.Add("institution_id", CreateValue(document.InstitutionId.ToString()));
                point.Payload.Add("institution_name", CreateValue(document.InstitutionName));
                point.Payload.Add("report_period", CreateValue(document.ReportPeriod));
                point.Payload.Add("report_type", CreateValue(document.ReportType));
                point.Payload.Add("institution_type", CreateValue(document.Metadata.InstitutionType));
                point.Payload.Add("region", CreateValue(document.Metadata.Region));
                point.Payload.Add("student_count", CreateValue(document.Metadata.StudentCount));
                point.Payload.Add("overall_score", CreateValue(document.Metadata.OverallScore));
                point.Payload.Add("national_rank", CreateValue(document.Metadata.NationalRank ?? 0));
                point.Payload.Add("document", CreateValue(JsonSerializer.Serialize(document)));

                var response = await _qdrantService.UpsertPointsAsync(COLLECTION_NAME, new[] { point });

                if (response.Success)
                {
                    _logger.LogInformation("Successfully stored analytics document for institution {InstitutionId}", 
                        document.InstitutionId);
                    
                    // Cache the document for quick access
                    var cacheKey = $"analytics_doc_{document.InstitutionId}";
                    _cache.Set(cacheKey, document, _cacheExpiration);
                    
                    return document;
                }

                throw new InvalidOperationException($"Failed to store document in Qdrant: {response.Message}");
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                var delay = baseDelayMs * (int)Math.Pow(2, attempt - 1);
                _logger.LogWarning(ex, "Failed to store analytics document on attempt {Attempt}/{MaxRetries}. Retrying in {Delay}ms", 
                    attempt, maxRetries, delay);
                
                await Task.Delay(delay);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store analytics document after {MaxRetries} attempts", maxRetries);
                throw;
            }
        }

        throw new InvalidOperationException("Should not reach here");
    }

    public async Task<AnalyticsDocument?> GetAnalyticsDocumentAsync(Guid institutionId, string? reportPeriod = null)
    {
        // Check cache first
        var cacheKey = $"analytics_doc_{institutionId}_{reportPeriod ?? "latest"}";
        if (_cache.TryGetValue(cacheKey, out AnalyticsDocument? cachedDoc))
        {
            return cachedDoc;
        }

        try
        {
            var filter = new Filter();

            filter.Must.Add(new Condition
            {
                Field = new FieldCondition
                {
                    Key = "institution_id",
                    Match = new Match
                    {
                        Text = institutionId.ToString()
                    }
                }
            });

            if (!string.IsNullOrEmpty(reportPeriod))
            {
                filter.Must.Add(new Condition
                {
                    Field = new FieldCondition
                    {
                        Key = "report_period",
                        Match = new Match
                        {
                            Text = reportPeriod
                        }
                    }
                });
            }

            // Use zero vector for filtering (vector is ignored when filtering)
            var results = await _qdrantService.SearchAsync(
                collectionName: COLLECTION_NAME,
                vector: new float[768],
                limit: 1,
                filter: filter
            );

            if (!results.Any())
                return null;

            var docJson = results.First().Payload["document"].StringValue;
            var document = JsonSerializer.Deserialize<AnalyticsDocument>(docJson!);
            
            // Cache the result
            if (document != null)
            {
                _cache.Set(cacheKey, document, _cacheExpiration);
            }
            
            return document;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get analytics document for institution {InstitutionId}", institutionId);
            return null;
        }
    }

    public async Task<List<AnalyticsDocument>> SearchSimilarInstitutionsAsync(Guid institutionId, int limit = 5)
    {
        var cacheKey = $"similar_institutions_{institutionId}_{limit}";
        if (_cache.TryGetValue(cacheKey, out List<AnalyticsDocument>? cachedResults))
        {
            return cachedResults!;
        }

        try
        {
            var sourceDocument = await GetAnalyticsDocumentAsync(institutionId);
            if (sourceDocument == null)
                return new List<AnalyticsDocument>();

            var sourceText = CreateCombinedText(sourceDocument);
            var sourceEmbedding = await _embeddingService.GenerateEmbeddingAsync(sourceText);

            var filter = new Filter();
            filter.MustNot.Add(new Condition
            {
                Field = new FieldCondition
                {
                    Key = "institution_id",
                    Match = new Match
                    {
                        Text = institutionId.ToString()
                    }
                }
            });

            var results = await _qdrantService.SearchAsync(
                collectionName: COLLECTION_NAME,
                vector: sourceEmbedding,
                limit: (uint)limit,
                filter: filter
            );

            var documents = results.Select(r => JsonSerializer.Deserialize<AnalyticsDocument>(
                r.Payload["document"].StringValue!)!)
                .Where(doc => doc != null)
                .ToList();

            // Cache the results
            _cache.Set(cacheKey, documents, _cacheExpiration);
            
            return documents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search similar institutions for {InstitutionId}", institutionId);
            return new List<AnalyticsDocument>();
        }
    }

    public async Task<List<AnalyticsDocument>> SearchByQueryAsync(string query, int limit = 10)
    {
        try
        {
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);

            var results = await _qdrantService.SearchAsync(
                collectionName: COLLECTION_NAME,
                vector: queryEmbedding,
                limit: (uint)limit
            );

            return results.Select(r => JsonSerializer.Deserialize<AnalyticsDocument>(
                r.Payload["document"].StringValue!)!)
                .Where(doc => doc != null)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search by query: {Query}", query);
            return new List<AnalyticsDocument>();
        }
    }

    public async Task<string> GenerateInsightAsync(string question)
    {
        try
        {
            var relevantDocuments = await SearchByQueryAsync(question, 5);

            if (!relevantDocuments.Any())
                return "I don't have enough data to answer that question. Please ensure analytics reports have been generated.";

            var context = relevantDocuments.Select(doc =>
                $"Institution: {doc.InstitutionName} (Score: {doc.Metadata.OverallScore})\n" +
                $"Summary: {doc.Content.ExecutiveSummary}\n" +
                $"Achievements: {string.Join(", ", doc.Content.KeyAchievements)}\n" +
                $"Analysis: {doc.Content.PerformanceAnalysis}")
                .ToList();

            return await _ollama.GenerateInsightAsync(question, context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate insight for question: {Question}", question);
            return "I encountered an error while analyzing the data. Please try again.";
        }
    }

    public async Task<string> AskQuestionAsync(string question, Guid? institutionId = null)
    {
        return await GenerateInsightAsync(question);
    }

    public async Task<List<string>> GetAIRecommendationsAsync(Guid institutionId)
    {
        var cacheKey = $"recommendations_{institutionId}";
        if (_cache.TryGetValue(cacheKey, out List<string>? cachedRecs))
        {
            return cachedRecs!;
        }

        try
        {
            var institutionDoc = await GetAnalyticsDocumentAsync(institutionId);
            if (institutionDoc == null)
                return new List<string> { "No analytics data available for this institution." };

            var similarInstitutions = await SearchSimilarInstitutionsAsync(institutionId, 5);
            var higherPerforming = similarInstitutions
                .Where(doc => doc.Metadata.OverallScore > institutionDoc.Metadata.OverallScore)
                .ToList();

            var recommendations = await _ollama.GenerateRecommendationsAsync(institutionDoc, higherPerforming);
            
            // Cache recommendations
            _cache.Set(cacheKey, recommendations, _cacheExpiration);
            
            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate AI recommendations for {InstitutionId}", institutionId);
            return new List<string> { "Unable to generate recommendations at this time." };
        }
    }

    public async Task<string> GenerateExecutiveSummaryAsync(InstitutionAnalyticsReport report)
    {
        try
        {
            var prompt = $"""
                Generate a professional executive summary for this educational institution's performance report:
                
                Institution Details:
                - Overall Academic Score: {report.OverallAcademicScore:F1}
                - Year-over-year Growth: {report.YearOverYearGrowth:F1}%
                - Total Enrollment: {report.TotalEnrollments}
                - Enrollment Growth: {report.EnrollmentGrowthRate:F1}%
                - Attendance Rate: {report.AverageAttendanceRate:F1}%
                - Student-Teacher Ratio: {report.StudentTeacherRatio:F1}:1
                - Teacher Retention: {report.TeacherRetentionRate:F1}%
                - Performance Category: {report.OverallPerformanceCategory}
                
                Create a concise, professional executive summary (2-3 paragraphs) highlighting key performance indicators, 
                notable achievements, and overall institutional health.
                """;

            return await _ollama.GenerateChatResponseAsync(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate executive summary for report {ReportId}", report.Id);
            return GenerateBasicExecutiveSummary(report);
        }
    }

    public async Task<InstitutionComparisonDto> CompareInstitutionsAsync(Guid institutionId1, Guid institutionId2)
    {
        var cacheKey = $"comparison_{institutionId1}_{institutionId2}";
        if (_cache.TryGetValue(cacheKey, out InstitutionComparisonDto? cachedComparison))
        {
            return cachedComparison!;
        }

        var doc1 = await GetAnalyticsDocumentAsync(institutionId1);
        var doc2 = await GetAnalyticsDocumentAsync(institutionId2);

        if (doc1 == null || doc2 == null)
            throw new ArgumentException("One or both institutions not found");

        try
        {
            var prompt = $"""
                Compare these two educational institutions and provide a detailed analysis:
                
                Institution 1: {doc1.InstitutionName}
                - Overall Score: {doc1.Metadata.OverallScore}
                - Student Count: {doc1.Metadata.StudentCount}
                - Type: {doc1.Metadata.InstitutionType}
                - Achievements: {string.Join(", ", doc1.Content.KeyAchievements)}
                
                Institution 2: {doc2.InstitutionName}
                - Overall Score: {doc2.Metadata.OverallScore}
                - Student Count: {doc2.Metadata.StudentCount}
                - Type: {doc2.Metadata.InstitutionType}
                - Achievements: {string.Join(", ", doc2.Content.KeyAchievements)}
                
                Provide a structured comparison focusing on:
                1. Academic Performance
                2. Strengths and Weaknesses
                3. Key Differentiators
                4. Recommendations for each institution
                """;

            var comparison = await _ollama.GenerateChatResponseAsync(prompt);

            var result = new InstitutionComparisonDto(
                Institution1: new InstitutionSummaryDto(doc1.InstitutionId, doc1.InstitutionName, doc1.Metadata.OverallScore),
                Institution2: new InstitutionSummaryDto(doc2.InstitutionId, doc2.InstitutionName, doc2.Metadata.OverallScore),
                ComparisonAnalysis: comparison,
                GeneratedAt: DateTime.UtcNow
            );

            // Cache the comparison
            _cache.Set(cacheKey, result, _cacheExpiration);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate AI comparison for institutions {Id1} and {Id2}", institutionId1, institutionId2);
            
            // Return a basic comparison without AI
            var basicComparison = $"Institution 1 ({doc1.InstitutionName}) has a score of {doc1.Metadata.OverallScore:F1} " +
                                $"while Institution 2 ({doc2.InstitutionName}) has a score of {doc2.Metadata.OverallScore:F1}.";
            
            return new InstitutionComparisonDto(
                Institution1: new InstitutionSummaryDto(doc1.InstitutionId, doc1.InstitutionName, doc1.Metadata.OverallScore),
                Institution2: new InstitutionSummaryDto(doc2.InstitutionId, doc2.InstitutionName, doc2.Metadata.OverallScore),
                ComparisonAnalysis: basicComparison,
                GeneratedAt: DateTime.UtcNow
            );
        }
    }

    // Helper methods
    private async Task<AnalyticsDocument> CreateAnalyticsDocumentAsync(InstitutionAnalyticsReport sqlReport)
    {
        // OPTIMIZED: Use projection instead of loading full institution
        var institutionData = await _context.Institutions
            .Where(i => i.Id == sqlReport.InstitutionId)
            .Select(i => new
            {
                i.Name,
                i.Type,
                Country = i.Address.Country
            })
            .FirstOrDefaultAsync();

        return new AnalyticsDocument(
            Id: sqlReport.Id,
            InstitutionId: sqlReport.InstitutionId,
            InstitutionName: institutionData?.Name ?? "Unknown",
            ReportPeriod: $"{sqlReport.From:yyyy-MM-dd}_to_{sqlReport.To:yyyy-MM-dd}",
            ReportType: sqlReport.PeriodType.ToString(),
            Metadata: new AnalyticsMetadata(
                InstitutionType: institutionData?.Type.ToString() ?? "Unknown",
                Region: institutionData?.Country ?? "Unknown",
                StudentCount: sqlReport.TotalEnrollments,
                OverallScore: sqlReport.OverallAcademicScore,
                NationalRank: JsonSerializer.Deserialize<Dictionary<string, int>>(sqlReport.NationalRankings)
                    ?.GetValueOrDefault("Overall"),
                ReportDate: sqlReport.To
            ),
            Content: new AnalyticsContent(
                ExecutiveSummary: sqlReport.ExecutiveSummary ?? GenerateBasicExecutiveSummary(sqlReport),
                KeyAchievements: JsonSerializer.Deserialize<List<string>>(sqlReport.TopAchievements) ?? new(),
                PerformanceAnalysis: GeneratePerformanceAnalysis(sqlReport),
                ImprovementAreas: GenerateImprovementAreas(sqlReport),
                PeerComparisons: GeneratePeerComparisons(sqlReport),
                Recommendations: GenerateRecommendations(sqlReport)
            ),
            Metrics: JsonSerializer.Deserialize<Dictionary<string, decimal>>(sqlReport.SubjectPerformanceScores)
                ?? new Dictionary<string, decimal>(),
            CreatedAt: sqlReport.CreatedAt
        );
    }

    private string CreateCombinedText(AnalyticsDocument document)
    {
        return $"""
            Institution: {document.InstitutionName}
            Type: {document.Metadata.InstitutionType}
            Region: {document.Metadata.Region}
            Overall Score: {document.Metadata.OverallScore}
            
            Executive Summary: {document.Content.ExecutiveSummary}
            
            Key Achievements: {string.Join(", ", document.Content.KeyAchievements)}
            
            Performance Analysis: {document.Content.PerformanceAnalysis}
            
            Improvement Areas: {string.Join(", ", document.Content.ImprovementAreas)}
            
            Recommendations: {string.Join(", ", document.Content.Recommendations)}
            """;
    }

    private static Value CreateValue(object obj)
    {
        return obj switch
        {
            string s => new Value { StringValue = s },
            int i => new Value { IntegerValue = i },
            long l => new Value { IntegerValue = l },
            double d => new Value { DoubleValue = d },
            decimal dec => new Value { DoubleValue = (double)dec },
            bool b => new Value { BoolValue = b },
            _ => new Value { StringValue = obj?.ToString() ?? "" }
        };
    }

    // Helper methods for content generation
    private string GenerateBasicExecutiveSummary(InstitutionAnalyticsReport report) =>
        $"Academic performance score of {report.OverallAcademicScore:F1} with {report.YearOverYearGrowth:F1}% year-over-year growth. " +
        $"Current enrollment: {report.TotalEnrollments} students with {report.EnrollmentGrowthRate:F1}% growth rate. " +
        $"Overall performance category: {report.OverallPerformanceCategory}.";

    private string GeneratePerformanceAnalysis(InstitutionAnalyticsReport report) =>
        $"The institution demonstrates {report.OverallPerformanceCategory} performance with a student-teacher ratio of {report.StudentTeacherRatio:F1}:1 " +
        $"and {report.AverageAttendanceRate:F1}% average attendance rate.";

    private List<string> GenerateImprovementAreas(InstitutionAnalyticsReport report)
    {
        var areas = new List<string>();

        if (report.TeacherRetentionRate < 85) areas.Add("Teacher retention and satisfaction");
        if (report.AverageAttendanceRate < 90) areas.Add("Student attendance improvement");
        if (report.YearOverYearGrowth < 0) areas.Add("Academic performance growth");
        if (report.EnrollmentGrowthRate < 0) areas.Add("Student enrollment and retention");

        return areas;
    }

    private string GeneratePeerComparisons(InstitutionAnalyticsReport report) =>
        $"Compared to national averages, performance is {(report.OverallAcademicScore > 75 ? "above" : "below")} average.";

    private List<string> GenerateRecommendations(InstitutionAnalyticsReport report)
    {
        var recommendations = new List<string>();

        if (report.EnrollmentGrowthRate < 5)
            recommendations.Add("Enhance marketing and outreach programs to boost enrollment");

        if (report.TeacherRetentionRate < 85)
            recommendations.Add("Implement teacher development and retention programs");

        if (report.AverageAttendanceRate < 90)
            recommendations.Add("Develop student engagement and attendance improvement initiatives");

        return recommendations;
    }
}