using System;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Data.Models.Analytics;

public class MarketAnalyticsReport : BaseEntity
{
    public Guid Id { get; set; }

    // Report metadata
    public required ReportType ReportType { get; set; }
    public required ReportPeriodType PeriodType { get; set; }
    public required string ReportPeriod { get; set; } // e.g., "2024-Q1", "2024-Fall"

    // Market overview
    public int TotalInstitutions { get; set; }
    public int TotalStudents { get; set; }
    public decimal MarketGrowthRate { get; set; }
    public decimal AverageInstitutionScore { get; set; }

    // Top performers (JSON stored)
    public string EnrollmentLeaders { get; set; } = "[]"; // List<InstitutionRanking>
    public string AcademicLeaders { get; set; } = "[]"; // List<InstitutionRanking>
    public string FastestGrowing { get; set; } = "[]"; // List<InstitutionRanking>

    // Subject and major trends
    public string SubjectLeaders { get; set; } = "{}"; // Dictionary<string, List<InstitutionRanking>>
    public string TrendingMajors { get; set; } = "[]"; // List<MajorTrend>
    public string DecliningMajors { get; set; } = "[]"; // List<MajorTrend>

    // Geographic analysis
    public string RegionalBreakdown { get; set; } = "{}"; // Dictionary<string, RegionalStats>

    // Insights
    public string? MarketInsights { get; set; }
    public string? FutureProjections { get; set; }
}