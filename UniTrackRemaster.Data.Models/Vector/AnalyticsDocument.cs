using System;
using Qdrant.Client.Grpc;

namespace UniTrackRemaster.Data.Models.Vector;

public record AnalyticsDocument(
    Guid Id,
    Guid InstitutionId,
    string InstitutionName,
    string ReportPeriod,
    string ReportType,
    AnalyticsMetadata Metadata,
    AnalyticsContent Content,
    Dictionary<string, decimal> Metrics,
    DateTime CreatedAt
);
public record AnalyticsMetadata(
    string InstitutionType,
    string Region,
    int StudentCount,
    decimal OverallScore,
    int? NationalRank,
    DateTime ReportDate
);
public record AnalyticsContent(
    string ExecutiveSummary,
    List<string> KeyAchievements,
    string PerformanceAnalysis,
    List<string> ImprovementAreas,
    string PeerComparisons,
    List<string> Recommendations
);

public record RegionalStats(
    string Region,
    int InstitutionCount,
    int TotalStudents,
    decimal AverageScore,
    decimal GrowthRate
);

public class UpsertResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int UpsertedCount { get; set; }
}

public class GetResponse
{
    public PointStruct? Point { get; set; }
    public bool Found { get; set; }
}

public class UpdateResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int DeletedCount { get; set; }
}