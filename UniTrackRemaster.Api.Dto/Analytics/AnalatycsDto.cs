using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Analytics;

// Main analytics dashboard DTO
public record AnalyticsDashboardDto(
    InstitutionOverviewDto Overview,
    PerformanceMetricsDto Performance,
    List<AchievementDto> Achievements,
    List<TrendDto> Trends,
    ComparisonDataDto Comparisons,
    List<RecommendationDto> Recommendations,
    DateTime GeneratedAt
);

// Institution overview
public record InstitutionOverviewDto(
    Guid InstitutionId,
    string Name,
    string Type,
    int StudentCount,
    decimal OverallScore,
    string PerformanceCategory,
    int? NationalRank,
    int? RegionalRank
);

// Performance metrics
public record PerformanceMetricsDto(
    decimal AcademicScore,
    decimal YearOverYearGrowth,
    decimal AttendanceRate,
    decimal TeacherRetention,
    decimal StudentTeacherRatio,
    Dictionary<string, decimal> SubjectScores
);

// Achievement representation
public record AchievementDto(
    string Title,
    string Description,
    string Category,
    DateTime AchievedDate,
    string? Icon = null
);

// Trend data
public record TrendDto(
    string Name,
    string Category,
    decimal CurrentValue,
    decimal PreviousValue,
    decimal ChangePercent,
    TrendDirection Direction,
    List<TrendDataPointDto> DataPoints
);

public record TrendDataPointDto(
    DateTime Date,
    decimal Value,
    string? Label = null
);

// Comparison data
public record ComparisonDataDto(
    List<PeerComparisonDto> PeerComparisons,
    BenchmarkDataDto Benchmarks,
    RankingDataDto Rankings
);

public record PeerComparisonDto(
    Guid InstitutionId,
    string Name,
    decimal Score,
    decimal Similarity,
    string Type,
    Dictionary<string, decimal> KeyMetrics
);

public record BenchmarkDataDto(
    decimal NationalAverage,
    decimal RegionalAverage,
    decimal InstitutionTypeAverage,
    Dictionary<string, decimal> SubjectBenchmarks
);

public record RankingDataDto(
    int? NationalRank,
    int? RegionalRank,
    int? TypeRank,
    int TotalInstitutions,
    Dictionary<string, int> SubjectRankings
);

// AI-generated recommendations
public record RecommendationDto(
    string Title,
    string Description,
    string Category,
    Priority Priority,
    List<string> ActionItems,
    string? ExpectedOutcome = null
);

public enum Priority
{
    Low,
    Medium,
    High,
    Critical
}

// Report summary for lists
public record ReportSummaryDto(
    Guid Id,
    string Title,
    string Period,
    ReportPeriodType PeriodType,
    decimal OverallScore,
    string PerformanceCategory,
    DateTime CreatedAt,
    bool IsPublic
);

// Market analytics DTO
public record MarketAnalyticsDto(
    string ReportPeriod,
    MarketOverviewDto Overview,
    List<InstitutionRankingDto> TopPerformers,
    List<MajorTrendDto> MajorTrends,
    List<RegionalStatsDto> RegionalData,
    string MarketInsights,
    DateTime GeneratedAt
);

public record MarketOverviewDto(
    int TotalInstitutions,
    int TotalStudents,
    decimal AverageScore,
    decimal MarketGrowthRate,
    Dictionary<string, int> InstitutionsByType
);

public record InstitutionRankingDto(
    Guid Id,
    string Name,
    string Type,
    decimal Score,
    int Rank,
    decimal ChangeFromPrevious,
    string? Category = null
);

public record MajorTrendDto(
    string Name,
    int CurrentEnrollment,
    decimal GrowthRate,
    TrendDirection Trend,
    int InstitutionsOffering,
    decimal AverageScore
);

public record RegionalStatsDto(
    string Region,
    int InstitutionCount,
    int StudentCount,
    decimal AverageScore,
    decimal GrowthRate
);

// Chat/Q&A DTOs
public record ChatQueryDto(
    string Question,
    Guid? InstitutionId = null,
    string? Context = null
);

public record ChatResponseDto(
    string Answer,
    List<string> Sources,
    decimal Confidence,
    DateTime GeneratedAt,
    string? FollowUpSuggestions = null
);

// Report generation DTOs
public record GenerateReportRequestDto(
    Guid InstitutionId,
    ReportPeriodType PeriodType,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    bool IncludeAIInsights = true,
    bool GenerateRecommendations = true
);

public record ReportJobDto(
    Guid Id,
    Guid? InstitutionId,
    string InstitutionName,
    ReportType ReportType,
    ReportPeriodType PeriodType,
    JobStatus Status,
    DateTime ScheduledFor,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    Guid? ReportId,
    string? ErrorMessage,
    decimal? Progress = null
);

// Comparison DTOs
public record ComparisonRequestDto(
    Guid Institution1Id,
    Guid Institution2Id,
    string? FocusArea = null
);

// Settings DTOs
public record VisibilitySettingsDto(
    bool AllowPublicSharing,
    bool ShowInMarketReports,
    bool AllowPeerComparison,
    bool HideStudentCount,
    bool HideFinancialData,
    string? CustomTitle,
    string? CustomDescription,
    List<string> HighlightedAchievements
);

// Analytics filters
public record AnalyticsFilterDto(
    DateTime? FromDate,
    DateTime? ToDate,
    ReportPeriodType? PeriodType,
    string? InstitutionType,
    string? Region,
    decimal? MinScore,
    decimal? MaxScore,
    List<string>? Categories
);