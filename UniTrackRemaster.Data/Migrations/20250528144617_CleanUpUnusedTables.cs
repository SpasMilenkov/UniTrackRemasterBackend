using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class CleanUpUnusedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schools_SchoolReports_SchoolReportId",
                table: "Schools");

            migrationBuilder.DropForeignKey(
                name: "FK_Universities_UniversityReports_UniversityReportId",
                table: "Universities");

            migrationBuilder.DropTable(
                name: "AcademicalReports");

            migrationBuilder.DropTable(
                name: "ParentTeacherMeeting");

            migrationBuilder.DropTable(
                name: "ReportEntry");

            migrationBuilder.DropTable(
                name: "SchoolReports");

            migrationBuilder.DropTable(
                name: "UniversityReports");

            migrationBuilder.DropTable(
                name: "PersonalReports");

            migrationBuilder.DropIndex(
                name: "IX_Universities_UniversityReportId",
                table: "Universities");

            migrationBuilder.DropIndex(
                name: "IX_Schools_SchoolReportId",
                table: "Schools");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParentTeacherMeeting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeRoomTeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    ScheduledTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentTeacherMeeting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParentTeacherMeeting_HomeRoomTeacher_HomeRoomTeacherId",
                        column: x => x.HomeRoomTeacherId,
                        principalTable: "HomeRoomTeacher",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParentTeacherMeeting_Parents_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Parents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicYear = table.Column<string>(type: "text", nullable: false),
                    AttendanceRate = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    GPA = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalCredits = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalReports_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchoolReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DetailedDescription = table.Column<string>(type: "text", nullable: false),
                    From = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NumericalRating = table.Column<decimal>(type: "numeric", nullable: false),
                    ShortDescription = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    To = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UniversityReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DetailedDescription = table.Column<string>(type: "text", nullable: false),
                    From = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NumericalRating = table.Column<decimal>(type: "numeric", nullable: false),
                    ShortDescription = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    To = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniversityReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonalReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MetricName = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportEntry_PersonalReports_PersonalReportId",
                        column: x => x.PersonalReportId,
                        principalTable: "PersonalReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AcademicalReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DetailedDescription = table.Column<string>(type: "text", nullable: false),
                    From = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NumericalRating = table.Column<decimal>(type: "numeric", nullable: false),
                    SchoolReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    ShortDescription = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    To = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UniversityReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicalReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcademicalReports_SchoolReports_SchoolReportId",
                        column: x => x.SchoolReportId,
                        principalTable: "SchoolReports",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AcademicalReports_UniversityReports_UniversityReportId",
                        column: x => x.UniversityReportId,
                        principalTable: "UniversityReports",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Universities_UniversityReportId",
                table: "Universities",
                column: "UniversityReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schools_SchoolReportId",
                table: "Schools",
                column: "SchoolReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AcademicalReports_SchoolReportId",
                table: "AcademicalReports",
                column: "SchoolReportId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicalReports_UniversityReportId",
                table: "AcademicalReports",
                column: "UniversityReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentTeacherMeeting_HomeRoomTeacherId",
                table: "ParentTeacherMeeting",
                column: "HomeRoomTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentTeacherMeeting_ParentId",
                table: "ParentTeacherMeeting",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalReports_StudentId",
                table: "PersonalReports",
                column: "StudentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportEntry_PersonalReportId",
                table: "ReportEntry",
                column: "PersonalReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_SchoolReports_SchoolReportId",
                table: "Schools",
                column: "SchoolReportId",
                principalTable: "SchoolReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Universities_UniversityReports_UniversityReportId",
                table: "Universities",
                column: "UniversityReportId",
                principalTable: "UniversityReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
