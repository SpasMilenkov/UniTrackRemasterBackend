using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHybridAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InstitutionAnalyticsReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    InstitutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    From = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    To = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodType = table.Column<string>(type: "text", nullable: false),
                    OverallAcademicScore = table.Column<decimal>(type: "numeric", nullable: false),
                    YearOverYearGrowth = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalEnrollments = table.Column<int>(type: "integer", nullable: false),
                    EnrollmentGrowthRate = table.Column<decimal>(type: "numeric", nullable: false),
                    AverageAttendanceRate = table.Column<decimal>(type: "numeric", nullable: false),
                    StudentTeacherRatio = table.Column<decimal>(type: "numeric", nullable: false),
                    TeacherRetentionRate = table.Column<decimal>(type: "numeric", nullable: false),
                    ExtracurricularParticipation = table.Column<decimal>(type: "numeric", nullable: false),
                    OverallPerformanceCategory = table.Column<string>(type: "text", nullable: false),
                    SubjectPerformanceScores = table.Column<string>(type: "text", nullable: false, defaultValue: "{}"),
                    DepartmentRankings = table.Column<string>(type: "text", nullable: false, defaultValue: "{}"),
                    PopularMajors = table.Column<string>(type: "text", nullable: false, defaultValue: "{}"),
                    MajorGrowthRates = table.Column<string>(type: "text", nullable: false, defaultValue: "{}"),
                    NationalRankings = table.Column<string>(type: "text", nullable: false, defaultValue: "{}"),
                    RegionalRankings = table.Column<string>(type: "text", nullable: false, defaultValue: "{}"),
                    TopAchievements = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                    FastestGrowingAreas = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                    StrongestSubjects = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                    ExecutiveSummary = table.Column<string>(type: "text", nullable: true),
                    AIGeneratedInsights = table.Column<string>(type: "text", nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionAnalyticsReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstitutionAnalyticsReports_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarketAnalyticsReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    ReportType = table.Column<string>(type: "text", nullable: false),
                    PeriodType = table.Column<string>(type: "text", nullable: false),
                    ReportPeriod = table.Column<string>(type: "text", nullable: false),
                    TotalInstitutions = table.Column<int>(type: "integer", nullable: false),
                    TotalStudents = table.Column<int>(type: "integer", nullable: false),
                    MarketGrowthRate = table.Column<decimal>(type: "numeric", nullable: false),
                    AverageInstitutionScore = table.Column<decimal>(type: "numeric", nullable: false),
                    EnrollmentLeaders = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                    AcademicLeaders = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                    FastestGrowing = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                    SubjectLeaders = table.Column<string>(type: "text", nullable: false, defaultValue: "{}"),
                    TrendingMajors = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                    DecliningMajors = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                    RegionalBreakdown = table.Column<string>(type: "text", nullable: false, defaultValue: "{}"),
                    MarketInsights = table.Column<string>(type: "text", nullable: true),
                    FutureProjections = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketAnalyticsReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportGenerationJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    InstitutionId = table.Column<Guid>(type: "uuid", nullable: true),
                    PeriodType = table.Column<string>(type: "text", nullable: false),
                    ReportType = table.Column<string>(type: "text", nullable: false),
                    ScheduledFor = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GeneratedReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    ProcessingLogs = table.Column<string>(type: "text", nullable: true),
                    JobParameters = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportGenerationJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportGenerationJobs_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ReportVisibilitySettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    InstitutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowPublicSharing = table.Column<bool>(type: "boolean", nullable: false),
                    ShowInMarketReports = table.Column<bool>(type: "boolean", nullable: false),
                    AllowPeerComparison = table.Column<bool>(type: "boolean", nullable: false),
                    CustomTitle = table.Column<string>(type: "text", nullable: true),
                    CustomDescription = table.Column<string>(type: "text", nullable: true),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    HighlightedAchievements = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                    CustomMetrics = table.Column<string>(type: "text", nullable: false, defaultValue: "{}"),
                    HideStudentCount = table.Column<bool>(type: "boolean", nullable: false),
                    HideFinancialData = table.Column<bool>(type: "boolean", nullable: false),
                    HideDetailedBreakdown = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportVisibilitySettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportVisibilitySettings_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionAnalyticsReports_InstitutionId",
                table: "InstitutionAnalyticsReports",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionAnalyticsReports_InstitutionId_PeriodType_From",
                table: "InstitutionAnalyticsReports",
                columns: new[] { "InstitutionId", "PeriodType", "From" });

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionAnalyticsReports_OverallAcademicScore",
                table: "InstitutionAnalyticsReports",
                column: "OverallAcademicScore");

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionAnalyticsReports_PeriodType",
                table: "InstitutionAnalyticsReports",
                column: "PeriodType");

            migrationBuilder.CreateIndex(
                name: "IX_MarketAnalyticsReports_PeriodType",
                table: "MarketAnalyticsReports",
                column: "PeriodType");

            migrationBuilder.CreateIndex(
                name: "IX_MarketAnalyticsReports_ReportType",
                table: "MarketAnalyticsReports",
                column: "ReportType");

            migrationBuilder.CreateIndex(
                name: "IX_MarketAnalyticsReports_ReportType_PeriodType_ReportPeriod",
                table: "MarketAnalyticsReports",
                columns: new[] { "ReportType", "PeriodType", "ReportPeriod" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportGenerationJobs_InstitutionId",
                table: "ReportGenerationJobs",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportGenerationJobs_PeriodType_Status",
                table: "ReportGenerationJobs",
                columns: new[] { "PeriodType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportGenerationJobs_ScheduledFor",
                table: "ReportGenerationJobs",
                column: "ScheduledFor");

            migrationBuilder.CreateIndex(
                name: "IX_ReportGenerationJobs_Status",
                table: "ReportGenerationJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ReportVisibilitySettings_InstitutionId",
                table: "ReportVisibilitySettings",
                column: "InstitutionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstitutionAnalyticsReports");

            migrationBuilder.DropTable(
                name: "MarketAnalyticsReports");

            migrationBuilder.DropTable(
                name: "ReportGenerationJobs");

            migrationBuilder.DropTable(
                name: "ReportVisibilitySettings");
        }
    }
}
