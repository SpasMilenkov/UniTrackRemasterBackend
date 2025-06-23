using System;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Data.Models.Vector;

public record MajorTrend(
    string MajorName,
    int CurrentEnrollment,
    decimal GrowthRate,
    TrendDirection Trend,
    int InstitutionsOffering,
    decimal AveragePerformanceScore
);