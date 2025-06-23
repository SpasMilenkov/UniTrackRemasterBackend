using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Analytics;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Vector;

namespace UniTrackRemaster.Services.Reports;

public class PdfService : IPdfService
{
    private readonly UniTrackDbContext _context;
    private readonly ILogger<PdfService> _logger;

    public PdfService(UniTrackDbContext context, ILogger<PdfService> logger)
    {
        _context = context;
        _logger = logger;

        // Configure QuestPDF license (Community license is free)
        Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateInstitutionAnalyticsReportAsync(InstitutionAnalyticsReport report)
    {
        try
        {
            _logger.LogInformation("Generating PDF for analytics report {ReportId}", report.Id);

            // Load related data
            var institution = await _context.Institutions
                .FirstOrDefaultAsync(i => i.Id == report.InstitutionId);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    FontManager.RegisterFont(File.OpenRead("/home/spasmilenkov/repos/UniTrackRemaster/UniTrackRemasterBackend/Services/UniTrackRemaster.Services.Analytics/Fonts/Symbola.ttf")); // or NotoSansSymbols2.ttf

                    // Use FontFamilyFallback in the default style
                    page.DefaultTextStyle(x => x
                        .FontFamily("Symbola") // Fallback for emoji
                        .FontSize(11)
                    );

                    page.Header().Element(header =>
                        CreateHeader(header, institution?.Name ?? "Unknown Institution", report));
                    page.Content().Element(content => CreateMainContent(content, report, institution));
                    page.Footer().Element(footer => CreateFooter(footer));
                });
            });

            var pdfBytes = document.GeneratePdf();
            _logger.LogInformation("Successfully generated PDF for report {ReportId}, size: {Size} bytes",
                report.Id, pdfBytes.Length);

            return pdfBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PDF for report {ReportId}", report.Id);
            throw;
        }
    }

    public async Task<byte[]> GenerateMarketAnalyticsReportAsync(MarketAnalyticsReport report)
    {
        try
        {
            _logger.LogInformation("Generating market analytics PDF for period {ReportPeriod}", report.ReportPeriod);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Segoe UI"));

                    page.Header().Element(header => CreateMarketHeader(header, report));
                    page.Content().Element(content => CreateMarketContent(content, report));
                    page.Footer().Element(footer => CreateFooter(footer));
                });
            });

            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate market analytics PDF");
            throw;
        }
    }

    public async Task<byte[]> GenerateComparisonReportAsync(InstitutionAnalyticsReport report1,
        InstitutionAnalyticsReport report2)
    {
        try
        {
            _logger.LogInformation("Generating comparison PDF for reports {Report1} vs {Report2}",
                report1.Id, report2.Id);

            var institution1 = await _context.Institutions.FirstOrDefaultAsync(i => i.Id == report1.InstitutionId);
            var institution2 = await _context.Institutions.FirstOrDefaultAsync(i => i.Id == report2.InstitutionId);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Segoe UI"));

                    page.Header().Element(header => CreateComparisonHeader(header, institution1, institution2));
                    page.Content().Element(content =>
                        CreateComparisonContent(content, report1, report2, institution1, institution2));
                    page.Footer().Element(footer => CreateFooter(footer));
                });
            });

            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate comparison PDF");
            throw;
        }
    }

    private void CreateHeader(IContainer container, string institutionName, InstitutionAnalyticsReport report)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Analytics Report")
                        .FontSize(24)
                        .Bold()
                        .FontColor("#2563eb");

                    column.Item().Text(institutionName)
                        .FontSize(18)
                        .SemiBold()
                        .FontColor("#374151");

                    column.Item().Text($"Reporting Period: {report.From:MMM dd, yyyy} - {report.To:MMM dd, yyyy}")
                        .FontSize(12)
                        .FontColor("#6b7280");
                });

                row.ConstantItem(120).Column(column =>
                {
                    column.Item().AlignRight().Text($"Generated: {DateTime.Now:MMM dd, yyyy}")
                        .FontSize(10)
                        .FontColor("#6b7280");

                    column.Item().AlignRight().Text($"Period: {report.PeriodType}")
                        .FontSize(10)
                        .FontColor("#6b7280");
                });
            });

            // This is now a *separate* item in the same column, safe to use
            col.Item().PaddingTop(10).LineHorizontal(2).LineColor("#e5e7eb");
        });
    }


    private void CreateMainContent(IContainer container, InstitutionAnalyticsReport report, object? institution)
    {
        container.PaddingTop(20).Column(column =>
        {
            // Executive Summary
            column.Item().Element(content => CreateExecutiveSummary(content, report));

            column.Item().PaddingTop(20);

            // Key Metrics Dashboard
            column.Item().Element(content => CreateKeyMetrics(content, report));

            column.Item().PaddingTop(20);

            // Performance Analysis
            column.Item().Element(content => CreatePerformanceAnalysis(content, report));

            column.Item().PaddingTop(20);

            // Subject Performance
            column.Item().Element(content => CreateSubjectPerformance(content, report));

            column.Item().PaddingTop(20);

            // Achievements
            column.Item().Element(content => CreateAchievements(content, report));

            column.Item().PaddingTop(20);

            // AI Recommendations
            column.Item().Element(content => CreateRecommendations(content, report));
        });
    }

    private void CreateExecutiveSummary(IContainer container, InstitutionAnalyticsReport report)
    {
        container.Column(column =>
        {
            column.Item().Text("Executive Summary")
                .FontSize(16)
                .SemiBold()
                .FontColor("#1f2937");

            column.Item().PaddingTop(10).Background("#f8fafc").Padding(15).Column(summaryColumn =>
            {
                summaryColumn.Item().Text(report.ExecutiveSummary ?? GenerateDefaultSummary(report))
                    .FontSize(11)
                    .LineHeight(1.4f)
                    .FontColor("#374151");
            });
        });
    }

    private void CreateKeyMetrics(IContainer container, InstitutionAnalyticsReport report)
    {
        container.Column(column =>
        {
            column.Item().Text("Key Performance Indicators")
                .FontSize(16)
                .SemiBold()
                .FontColor("#1f2937");

            column.Item().PaddingTop(15).Row(row =>
            {
                // Academic Score
                row.RelativeItem().Element(content => CreateMetricCard(content,
                    "Overall Academic Score",
                    $"{report.OverallAcademicScore:F1}",
                    report.YearOverYearGrowth >= 0 ? "#10b981" : "#ef4444",
                    $"{report.YearOverYearGrowth:+0.0;-0.0}% YoY"));

                row.ConstantItem(15);

                // Enrollment
                row.RelativeItem().Element(content => CreateMetricCard(content,
                    "Total Enrollment",
                    report.TotalEnrollments.ToString("N0"),
                    report.EnrollmentGrowthRate >= 0 ? "#10b981" : "#ef4444",
                    $"{report.EnrollmentGrowthRate:+0.0;-0.0}% Growth"));

                row.ConstantItem(15);

                // Attendance
                row.RelativeItem().Element(content => CreateMetricCard(content,
                    "Attendance Rate",
                    $"{report.AverageAttendanceRate:F1}%",
                    "#3b82f6",
                    "Current Period"));
            });

            column.Item().PaddingTop(15).Row(row =>
            {
                // Teacher Retention
                row.RelativeItem().Element(content => CreateMetricCard(content,
                    "Teacher Retention",
                    $"{report.TeacherRetentionRate:F1}%",
                    "#8b5cf6",
                    "Annual Rate"));

                row.ConstantItem(15);

                // Student-Teacher Ratio
                row.RelativeItem().Element(content => CreateMetricCard(content,
                    "Student-Teacher Ratio",
                    $"{report.StudentTeacherRatio:F1}:1",
                    "#f59e0b",
                    "Current Period"));

                row.ConstantItem(15);

                // Performance Category
                row.RelativeItem().Element(content => CreateMetricCard(content,
                    "Performance Category",
                    report.OverallPerformanceCategory.ToString(),
                    GetPerformanceCategoryColor(report.OverallPerformanceCategory),
                    "Classification"));
            });
        });
    }

    private void CreateMetricCard(IContainer container, string title, string value, string color, string subtitle)
    {
        container.Border(1).BorderColor("#e5e7eb").Background("#ffffff").Padding(15).Column(column =>
        {
            column.Item().Text(title)
                .FontSize(10)
                .Medium()
                .FontColor("#6b7280");

            column.Item().PaddingTop(5).Text(value)
                .FontSize(20)
                .Bold()
                .FontColor(color);

            column.Item().PaddingTop(2).Text(subtitle)
                .FontSize(9)
                .FontColor("#9ca3af");
        });
    }

    private void CreatePerformanceAnalysis(IContainer container, InstitutionAnalyticsReport report)
    {
        container.Column(column =>
        {
            column.Item().Text("Performance Analysis")
                .FontSize(16)
                .SemiBold()
                .FontColor("#1f2937");

            column.Item().PaddingTop(15).Row(row =>
            {
                // Performance Trends Chart
                row.RelativeItem().Element(content => CreatePerformanceTrendChart(content, report));

                row.ConstantItem(20);

                // Performance Summary
                row.RelativeItem().Column(summaryColumn =>
                {
                    summaryColumn.Item().Text("Performance Insights")
                        .FontSize(12)
                        .SemiBold()
                        .FontColor("#374151");

                    summaryColumn.Item().PaddingTop(10).Column(insightColumn =>
                    {
                        CreateInsightItem(insightColumn, "Academic Growth",
                            GetGrowthInsight(report.YearOverYearGrowth));

                        CreateInsightItem(insightColumn, "Enrollment Trend",
                            GetEnrollmentInsight(report.EnrollmentGrowthRate));

                        CreateInsightItem(insightColumn, "Attendance Level",
                            GetAttendanceInsight(report.AverageAttendanceRate));

                        CreateInsightItem(insightColumn, "Teacher Stability",
                            GetRetentionInsight(report.TeacherRetentionRate));
                    });
                });
            });
        });
    }

    private void CreatePerformanceTrendChart(IContainer container, InstitutionAnalyticsReport report)
    {
        container.Border(1).BorderColor("#e5e7eb").Padding(15).Column(column =>
        {
            column.Item().Text("Performance Trend")
                .FontSize(12)
                .SemiBold()
                .FontColor("#374151");

            var metrics = new[]
            {
                ("Academic", (float)report.OverallAcademicScore, "#3b82f6"),
                ("Attendance", (float)report.AverageAttendanceRate, "#10b981"),
                ("Retention", (float)report.TeacherRetentionRate, "#8b5cf6")
            };

            const float maxHeight = 100f;

            column.Item().PaddingTop(10).Height(140).Row(row =>
            {
                foreach (var (label, value, color) in metrics)
                    row.RelativeItem().Column(col =>
                    {
                        var barHeightPercent = value / 100f;

                        col.Item().Height(100 - barHeightPercent * 100).Background(Colors.White); // Top spacing
                        col.Item().Height(barHeightPercent * 100).Background(color).AlignMiddle().AlignCenter()
                            .Text($"{value:F0}%")
                            .FontSize(8).FontColor(Colors.White);

                        col.Item().PaddingTop(5).AlignCenter().Text(label)
                            .FontSize(8).FontColor(Colors.Black);
                    });
            });
        });
    }


    private void CreateInsightItem(ColumnDescriptor column, string title, string insight)
    {
        column.Item().PaddingBottom(8).Row(row =>
        {
            row.ConstantItem(8).Background("#3b82f6").Height(8);
            row.ConstantItem(10);
            row.RelativeItem().Column(textColumn =>
            {
                textColumn.Item().Text(title)
                    .FontSize(10)
                    .Medium()
                    .FontColor("#374151");
                textColumn.Item().Text(insight)
                    .FontSize(9)
                    .FontColor("#6b7280");
            });
        });
    }

    private void CreateSubjectPerformance(IContainer container, InstitutionAnalyticsReport report)
    {
        var subjectScores = JsonSerializer.Deserialize<Dictionary<string, decimal>>(report.SubjectPerformanceScores)
                            ?? new Dictionary<string, decimal>();

        if (!subjectScores.Any()) return;

        container.Column(column =>
        {
            column.Item().Text("Subject Performance Breakdown")
                .FontSize(16)
                .SemiBold()
                .FontColor("#1f2937");

            column.Item().PaddingTop(15).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn(2);
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background("#f3f4f6").Padding(8).Text("Subject")
                        .SemiBold().FontSize(10);
                    header.Cell().Background("#f3f4f6").Padding(8).Text("Score")
                        .SemiBold().FontSize(10);
                    header.Cell().Background("#f3f4f6").Padding(8).Text("Performance")
                        .SemiBold().FontSize(10);
                });

                // Data rows
                foreach (var (subject, score) in subjectScores.OrderByDescending(x => x.Value))
                {
                    table.Cell().Border(0.5f).BorderColor("#e5e7eb").Padding(8).Text(subject).FontSize(10);
                    table.Cell().Border(0.5f).BorderColor("#e5e7eb").Padding(8).Text($"{score:F1}").FontSize(10);
                    table.Cell().Border(0.5f).BorderColor("#e5e7eb").Padding(8).Element(content =>
                        CreatePerformanceBar(content, (float)score));
                }
            });
        });
    }

    private void CreatePerformanceBar(IContainer container, float score)
    {
        var color = score >= 80 ? "#10b981" : score >= 60 ? "#f59e0b" : "#ef4444";
        var width = Math.Min(score, 100) / 100f;

        container.Height(12).Row(row =>
        {
            row.RelativeItem(width).Background(color).Height(12);
            row.RelativeItem(1 - width).Background("#f3f4f6").Height(12);
        });
    }

    private void CreateAchievements(IContainer container, InstitutionAnalyticsReport report)
    {
        var achievements = JsonSerializer.Deserialize<List<string>>(report.TopAchievements) ?? new List<string>();

        if (!achievements.Any()) return;

        container.Column(column =>
        {
            column.Item().Text("Key Achievements")
                .FontSize(16)
                .SemiBold()
                .FontColor("#1f2937");

            column.Item().PaddingTop(15).Column(achievementColumn =>
            {
                foreach (var achievement in achievements)
                    achievementColumn.Item().PaddingBottom(8).Row(row =>
                    {
                        row.ConstantItem(20).Text("🏆").FontSize(14);
                        row.RelativeItem().Text(achievement)
                            .FontSize(11)
                            .FontColor("#374151");
                    });
            });
        });
    }

private void CreateRecommendations(IContainer container, InstitutionAnalyticsReport report)
{
    // Wrap in a column so each child is scoped properly
    container.Column(col =>
    {
        // First item is the manual page break
        col.Item().PageBreak();

        // Second item is the actual recommendations section
        col.Item().Column(column =>
        {
            // Section title
            column.Item().Text("AI-Generated Recommendations")
                .FontSize(16)
                .SemiBold()
                .FontColor("#1f2937");

            // Introductory text
            column.Item().PaddingTop(10)
                .Text("Based on your institution's performance data and comparison with similar institutions, here are actionable recommendations for improvement:")
                .FontSize(10)
                .FontColor("#6b7280")
                .Italic();

            // Recommendation list
            column.Item().PaddingTop(15).Column(recColumn =>
            {
                var recommendations = GenerateRecommendations(report);

                foreach (var (category, items) in recommendations)
                {
                    recColumn.Item().PaddingBottom(15).Column(categoryColumn =>
                    {
                        categoryColumn.Item().Text(category)
                            .FontSize(12)
                            .SemiBold()
                            .FontColor("#1f2937");

                        categoryColumn.Item().PaddingTop(8).Column(itemColumn =>
                        {
                            foreach (var item in items)
                            {
                                itemColumn.Item().PaddingBottom(6).Row(row =>
                                {
                                    row.ConstantItem(15).Text("•").FontColor("#3b82f6").Bold();
                                    row.RelativeItem().Text(item)
                                        .FontSize(10)
                                        .LineHeight(1.3f)
                                        .FontColor("#374151");
                                });
                            }
                        });
                    });
                }
            });

            // Optional AI insights block
            if (!string.IsNullOrEmpty(report.AIGeneratedInsights))
            {
                column.Item().PaddingTop(20).Column(insightsColumn =>
                {
                    insightsColumn.Item().Text("Additional AI Insights")
                        .FontSize(12)
                        .SemiBold()
                        .FontColor("#1f2937");

                    insightsColumn.Item().PaddingTop(8)
                        .Background("#f0f9ff")
                        .Padding(12)
                        .Text(report.AIGeneratedInsights)
                        .FontSize(10)
                        .LineHeight(1.4f)
                        .FontColor("#374151");
                });
            }
        });
    });
}


    // Market Report Methods
    private void CreateMarketHeader(IContainer container, MarketAnalyticsReport report)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("Market Analytics Report")
                    .FontSize(24)
                    .Bold()
                    .FontColor("#2563eb");

                column.Item().Text($"Period: {report.ReportPeriod}")
                    .FontSize(14)
                    .FontColor("#6b7280");
            });

            row.ConstantItem(120).AlignRight().Text($"Generated: {DateTime.Now:MMM dd, yyyy}")
                .FontSize(10)
                .FontColor("#6b7280");
        });

        container.PaddingTop(10).LineHorizontal(2).LineColor("#e5e7eb");
    }

    private void CreateMarketContent(IContainer container, MarketAnalyticsReport report)
    {
        container.PaddingTop(20).Column(column =>
        {
            // Market Overview
            column.Item().Element(content => CreateMarketOverview(content, report));

            column.Item().PaddingTop(20);

            // Top Performers
            column.Item().Element(content => CreateTopPerformers(content, report));

            column.Item().PaddingTop(20);

            // Market Insights
            if (!string.IsNullOrEmpty(report.MarketInsights))
                column.Item().Element(content => CreateMarketInsights(content, report));
        });
    }

    private void CreateMarketOverview(IContainer container, MarketAnalyticsReport report)
    {
        container.Column(column =>
        {
            column.Item().Text("Market Overview")
                .FontSize(16)
                .SemiBold()
                .FontColor("#1f2937");

            column.Item().PaddingTop(15).Row(row =>
            {
                row.RelativeItem().Element(content => CreateMetricCard(content,
                    "Total Institutions",
                    report.TotalInstitutions.ToString("N0"),
                    "#3b82f6",
                    "In Market"));

                row.ConstantItem(15);

                row.RelativeItem().Element(content => CreateMetricCard(content,
                    "Total Students",
                    report.TotalStudents.ToString("N0"),
                    "#10b981",
                    "Enrolled"));

                row.ConstantItem(15);

                row.RelativeItem().Element(content => CreateMetricCard(content,
                    "Average Score",
                    $"{report.AverageInstitutionScore:F1}",
                    "#8b5cf6",
                    "Market Average"));

                row.ConstantItem(15);

                row.RelativeItem().Element(content => CreateMetricCard(content,
                    "Market Growth",
                    $"{report.MarketGrowthRate:+0.0;-0.0}%",
                    report.MarketGrowthRate >= 0 ? "#10b981" : "#ef4444",
                    "YoY Growth"));
            });
        });
    }

    private void CreateTopPerformers(IContainer container, MarketAnalyticsReport report)
    {
        var academicLeaders = JsonSerializer.Deserialize<List<InstitutionRanking>>(report.AcademicLeaders)
                              ?? new List<InstitutionRanking>();

        if (!academicLeaders.Any()) return;

        container.Column(column =>
        {
            column.Item().Text("Top Academic Performers")
                .FontSize(16)
                .SemiBold()
                .FontColor("#1f2937");

            column.Item().PaddingTop(15).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Background("#f3f4f6").Padding(8).Text("Rank").SemiBold().FontSize(10);
                    header.Cell().Background("#f3f4f6").Padding(8).Text("Institution").SemiBold().FontSize(10);
                    header.Cell().Background("#f3f4f6").Padding(8).Text("Score").SemiBold().FontSize(10);
                    header.Cell().Background("#f3f4f6").Padding(8).Text("Change").SemiBold().FontSize(10);
                });

                foreach (var leader in academicLeaders.Take(10))
                {
                    table.Cell().Border(0.5f).BorderColor("#e5e7eb").Padding(8)
                        .Text($"#{leader.Rank}").FontSize(10).SemiBold();
                    table.Cell().Border(0.5f).BorderColor("#e5e7eb").Padding(8)
                        .Text(leader.InstitutionName).FontSize(10);
                    table.Cell().Border(0.5f).BorderColor("#e5e7eb").Padding(8)
                        .Text($"{leader.Score:F1}").FontSize(10);
                    table.Cell().Border(0.5f).BorderColor("#e5e7eb").Padding(8)
                        .Text($"{leader.ChangeFromPrevious:+0.0;-0.0}").FontSize(10)
                        .FontColor(leader.ChangeFromPrevious >= 0 ? "#10b981" : "#ef4444");
                }
            });
        });
    }

    private void CreateMarketInsights(IContainer container, MarketAnalyticsReport report)
    {
        container.Column(column =>
        {
            column.Item().Text("Market Insights")
                .FontSize(16)
                .SemiBold()
                .FontColor("#1f2937");

            column.Item().PaddingTop(10).Background("#f8fafc").Padding(15).Text(report.MarketInsights!)
                .FontSize(11)
                .LineHeight(1.4f)
                .FontColor("#374151");
        });
    }

    // Comparison Report Methods
    private void CreateComparisonHeader(IContainer container, object? institution1, object? institution2)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("Institution Comparison Report")
                    .FontSize(24)
                    .Bold()
                    .FontColor("#2563eb");

                column.Item().Text("Comparative Performance Analysis")
                    .FontSize(14)
                    .FontColor("#6b7280");
            });

            row.ConstantItem(120).AlignRight().Text($"Generated: {DateTime.Now:MMM dd, yyyy}")
                .FontSize(10)
                .FontColor("#6b7280");
        });

        container.PaddingTop(10).LineHorizontal(2).LineColor("#e5e7eb");
    }

    private void CreateComparisonContent(IContainer container, InstitutionAnalyticsReport report1,
        InstitutionAnalyticsReport report2, object? institution1, object? institution2)
    {
        container.PaddingTop(20).Column(column =>
        {
            // Side-by-side comparison
            column.Item().Element(content => CreateSideBySideComparison(content, report1, report2));
        });
    }

    private void CreateSideBySideComparison(IContainer container, InstitutionAnalyticsReport report1,
        InstitutionAnalyticsReport report2)
    {
        container.Column(column =>
        {
            column.Item().Text("Performance Comparison")
                .FontSize(16)
                .SemiBold()
                .FontColor("#1f2937");

            column.Item().PaddingTop(15).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Background("#f3f4f6").Padding(8).Text("Metric").SemiBold();
                    header.Cell().Background("#f3f4f6").Padding(8).Text("Institution 1").SemiBold();
                    header.Cell().Background("#f3f4f6").Padding(8).Text("Institution 2").SemiBold();
                });

                var metrics = new[]
                {
                    ("Overall Score", $"{report1.OverallAcademicScore:F1}", $"{report2.OverallAcademicScore:F1}"),
                    ("Enrollment", $"{report1.TotalEnrollments:N0}", $"{report2.TotalEnrollments:N0}"),
                    ("Attendance Rate", $"{report1.AverageAttendanceRate:F1}%", $"{report2.AverageAttendanceRate:F1}%"),
                    ("Teacher Retention", $"{report1.TeacherRetentionRate:F1}%", $"{report2.TeacherRetentionRate:F1}%"),
                    ("Student-Teacher Ratio", $"{report1.StudentTeacherRatio:F1}:1",
                        $"{report2.StudentTeacherRatio:F1}:1")
                };

                foreach (var (metric, value1, value2) in metrics)
                {
                    table.Cell().Border(0.5f).BorderColor("#e5e7eb").Padding(8).Text(metric);
                    table.Cell().Border(0.5f).BorderColor("#e5e7eb").Padding(8).Text(value1);
                    table.Cell().Border(0.5f).BorderColor("#e5e7eb").Padding(8).Text(value2);
                }
            });
        });
    }

    private void CreateFooter(IContainer container)
    {
        container.AlignCenter().Text("Generated by UniTrack Analytics Platform")
            .FontSize(8)
            .FontColor("#9ca3af");
    }

    // Helper Methods
    private string GenerateDefaultSummary(InstitutionAnalyticsReport report)
    {
        return $"This analytics report covers the period from {report.From:MMMM yyyy} to {report.To:MMMM yyyy}. " +
               $"The institution demonstrates {report.OverallPerformanceCategory.ToString().ToLower()} performance " +
               $"with an overall academic score of {report.OverallAcademicScore:F1} and " +
               $"{(report.YearOverYearGrowth >= 0 ? "positive" : "negative")} year-over-year growth of " +
               $"{Math.Abs(report.YearOverYearGrowth):F1}%. Current enrollment stands at " +
               $"{report.TotalEnrollments:N0} students with an average attendance rate of " +
               $"{report.AverageAttendanceRate:F1}%.";
    }

    private string GetPerformanceCategoryColor(PerformanceCategory category)
    {
        return category switch
        {
            PerformanceCategory.Excellent => "#10b981",
            PerformanceCategory.Good => "#3b82f6",
            PerformanceCategory.Average => "#f59e0b",
            PerformanceCategory.BelowAverage => "#ef4444",
            PerformanceCategory.NeedsImprovement => "#dc2626",
            _ => "#6b7280"
        };
    }

    private string GetGrowthInsight(decimal growth)
    {
        return growth switch
        {
            >= 10 => "Exceptional growth trajectory",
            >= 5 => "Strong upward performance",
            >= 0 => "Stable performance maintained",
            >= -5 => "Slight decline, monitor closely",
            _ => "Significant decline, action needed"
        };
    }

    private string GetEnrollmentInsight(decimal growth)
    {
        return growth switch
        {
            >= 10 => "Rapid enrollment expansion",
            >= 5 => "Healthy enrollment growth",
            >= 0 => "Stable enrollment numbers",
            >= -5 => "Minor enrollment decline",
            _ => "Concerning enrollment drop"
        };
    }

    private string GetAttendanceInsight(decimal rate)
    {
        return rate switch
        {
            >= 95 => "Outstanding attendance levels",
            >= 90 => "Good attendance performance",
            >= 85 => "Acceptable attendance rates",
            >= 80 => "Below average attendance",
            _ => "Poor attendance requires intervention"
        };
    }

    private string GetRetentionInsight(decimal rate)
    {
        return rate switch
        {
            >= 90 => "Excellent teacher retention",
            >= 85 => "Good retention rates",
            >= 80 => "Moderate retention levels",
            >= 75 => "Below average retention",
            _ => "High turnover concern"
        };
    }

    private Dictionary<string, List<string>> GenerateRecommendations(InstitutionAnalyticsReport report)
    {
        var recommendations = new Dictionary<string, List<string>>();

        // Academic Performance
        var academic = new List<string>();
        if (report.OverallAcademicScore < 80)
        {
            academic.Add("Implement targeted tutoring programs for struggling students");
            academic.Add("Enhance teacher professional development initiatives");
            academic.Add("Review and update curriculum to align with best practices");
        }

        if (report.YearOverYearGrowth < 0)
        {
            academic.Add("Conduct detailed analysis of performance decline factors");
            academic.Add("Establish performance improvement task force");
        }

        if (academic.Any()) recommendations["Academic Excellence"] = academic;

        // Student Engagement
        var engagement = new List<string>();
        if (report.AverageAttendanceRate < 90)
        {
            engagement.Add("Develop attendance improvement campaigns");
            engagement.Add("Investigate barriers to regular attendance");
            engagement.Add("Implement attendance tracking and early intervention systems");
        }

        if (engagement.Any()) recommendations["Student Engagement"] = engagement;

        // Operational Excellence
        var operational = new List<string>();
        if (report.TeacherRetentionRate < 85)
        {
            operational.Add("Review teacher compensation and benefits packages");
            operational.Add("Enhance workplace culture and support systems");
            operational.Add("Implement mentorship programs for new teachers");
        }

        if (report.StudentTeacherRatio > 25)
        {
            operational.Add("Consider hiring additional teaching staff");
            operational.Add("Optimize class scheduling and resource allocation");
        }

        if (operational.Any()) recommendations["Operational Excellence"] = operational;

        // Growth & Development
        var growth = new List<string>();
        if (report.EnrollmentGrowthRate < 2)
        {
            growth.Add("Enhance marketing and community outreach efforts");
            growth.Add("Improve facilities and educational offerings");
            growth.Add("Strengthen partnerships with feeder schools");
        }

        growth.Add("Invest in technology and digital learning platforms");
        growth.Add("Expand extracurricular and enrichment programs");
        recommendations["Growth & Development"] = growth;

        return recommendations;
    }
}