using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum TrendDirection
{
    [Display(Name = "Increasing")]
    Increasing,
    [Display(Name = "Decreasing")]
    Decreasing,
    [Display(Name = "Stable")]
    Stable,
    [Display(Name = "Volatile")]
    Volatile
}

// Analytics specific enums
public enum ReportPeriodType
{
    [Display(Name = "Monthly")]
    Monthly,
    [Display(Name = "Quarterly")]
    Quarterly,
    [Display(Name = "Semester")]
    Semester,
    [Display(Name = "Yearly")]
    Yearly
}

public enum PerformanceCategory
{
    [Display(Name = "Excellent")]
    Excellent,
    [Display(Name = "Good")]
    Good,
    [Display(Name = "Average")]
    Average,
    [Display(Name = "Below Average")]
    BelowAverage,
    [Display(Name = "Needs Improvement")]
    NeedsImprovement
}

public enum ReportType
{
    [Display(Name = "Institution Analytics")]
    InstitutionAnalytics,
    [Display(Name = "Market Analytics")]
    MarketAnalytics,
    [Display(Name = "Comparative Analytics")]
    ComparativeAnalytics
}

public enum JobStatus
{
    [Display(Name = "Pending")]
    Pending,
    [Display(Name = "Running")]
    Running,
    [Display(Name = "Completed")]
    Completed,
    [Display(Name = "Failed")]
    Failed,
    [Display(Name = "Cancelled")]
    Cancelled
}