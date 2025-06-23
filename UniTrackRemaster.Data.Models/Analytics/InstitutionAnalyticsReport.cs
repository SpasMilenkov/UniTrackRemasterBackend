using System;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Models.Analytics;

public class InstitutionAnalyticsReport : BaseEntity
{
    public Guid Id { get; set; }
    public required Guid InstitutionId { get; set; }
    public Institution? Institution { get; set; }

    // Time period for the report
    public required DateTime From { get; set; }
    public required DateTime To { get; set; }
    public required ReportPeriodType PeriodType { get; set; }

    // Core metrics
    public decimal OverallAcademicScore { get; set; }
    public decimal YearOverYearGrowth { get; set; }
    public int TotalEnrollments { get; set; }
    public decimal EnrollmentGrowthRate { get; set; }
    public decimal AverageAttendanceRate { get; set; }
    public decimal StudentTeacherRatio { get; set; }
    public decimal TeacherRetentionRate { get; set; }
    public PerformanceCategory OverallPerformanceCategory { get; set; }

    // JSON stored data (as strings for EF Core)
    public string SubjectPerformanceScores { get; set; } = "{}"; // Dictionary<string, decimal>
    public string DepartmentRankings { get; set; } = "{}"; // Dictionary<string, int>
    public string PopularMajors { get; set; } = "{}"; // Dictionary<string, int>
    public string MajorGrowthRates { get; set; } = "{}"; // Dictionary<string, decimal>
    public string NationalRankings { get; set; } = "{}"; // Dictionary<string, int>
    public string RegionalRankings { get; set; } = "{}"; // Dictionary<string, int>

    // Achievements and highlights
    public string TopAchievements { get; set; } = "[]"; // List<string>
    public string FastestGrowingAreas { get; set; } = "[]"; // List<string>
    public string StrongestSubjects { get; set; } = "[]"; // List<string>

    // AI generated summary
    public string? ExecutiveSummary { get; set; }
    public string? AIGeneratedInsights { get; set; }

    // Visibility settings
    public bool IsPublic { get; set; } = false;
    public DateTime? PublishedAt { get; set; }
}