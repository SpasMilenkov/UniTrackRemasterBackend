using System;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Models.Analytics;

public class ReportVisibilitySettings : BaseEntity
{
    public Guid Id { get; set; }
    public required Guid InstitutionId { get; set; }
    public Institution? Institution { get; set; }

    // Public sharing settings
    public bool AllowPublicSharing { get; set; } = false;
    public bool ShowInMarketReports { get; set; } = true;
    public bool AllowPeerComparison { get; set; } = true;

    // Custom branding
    public string? CustomTitle { get; set; }
    public string? CustomDescription { get; set; }
    public string? LogoUrl { get; set; }

    // Data preferences
    public string HighlightedAchievements { get; set; } = "[]"; // List<string>
    public string CustomMetrics { get; set; } = "{}"; // Dictionary<string, object>

    // Privacy settings
    public bool HideStudentCount { get; set; } = false;
    public bool HideFinancialData { get; set; } = true;
    public bool HideDetailedBreakdown { get; set; } = false;
}
