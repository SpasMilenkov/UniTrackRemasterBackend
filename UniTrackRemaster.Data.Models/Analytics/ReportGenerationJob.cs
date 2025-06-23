using System;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Models.Analytics;

public class ReportGenerationJob : BaseEntity
{
    public Guid Id { get; set; }
    public Guid? InstitutionId { get; set; }
    public Institution? Institution { get; set; }

    // Job details
    public required ReportPeriodType PeriodType { get; set; }
    public required ReportType ReportType { get; set; }
    public required DateTime ScheduledFor { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Pending;

    // Time tracking
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Results
    public Guid? GeneratedReportId { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ProcessingLogs { get; set; }

    // Configuration
    public string? JobParameters { get; set; } // JSON configuration
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
}