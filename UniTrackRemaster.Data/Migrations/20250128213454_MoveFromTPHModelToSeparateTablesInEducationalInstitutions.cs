    using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveFromTPHModelToSeparateTablesInEducationalInstitutions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EducationalInstitutions_SchoolReports_SchoolReportId",
                table: "EducationalInstitutions");

            migrationBuilder.DropForeignKey(
                name: "FK_EducationalInstitutions_UniversityReports_UniversityReportId",
                table: "EducationalInstitutions");

            migrationBuilder.DropForeignKey(
                name: "FK_Faculties_EducationalInstitutions_UniversityId",
                table: "Faculties");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_EducationalInstitutions_SchoolId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_EducationalInstitutions_UniversityId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_EducationalInstitutions_SchoolReportId",
                table: "EducationalInstitutions");

            migrationBuilder.DropIndex(
                name: "IX_EducationalInstitutions_UniversityReportId",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "AcceptanceRate",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "Departments",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "ExtracurricularActivities",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "FocusAreas",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "GraduateCount",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "HasSpecialEducation",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "HasStudentHousing",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "HasUniform",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "InstitutionType",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "Levels",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "Programs",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "ResearchFunding",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "SchoolReportId",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "StudentCount",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "StudentTeacherRatio",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "UndergraduateCount",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "UniversityReportId",
                table: "EducationalInstitutions");

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    Level = table.Column<string>(type: "text", nullable: false),
                    StudentCount = table.Column<int>(type: "integer", nullable: false),
                    StudentTeacherRatio = table.Column<double>(type: "double precision", nullable: false),
                    HasSpecialEducation = table.Column<bool>(type: "boolean", nullable: false),
                    ExtracurricularActivities = table.Column<string>(type: "text", nullable: false),
                    HasUniform = table.Column<bool>(type: "boolean", nullable: false),
                    Programs = table.Column<string[]>(type: "text[]", nullable: true),
                    SchoolReportId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schools_EducationalInstitutions_Id",
                        column: x => x.Id,
                        principalTable: "EducationalInstitutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Schools_SchoolReports_SchoolReportId",
                        column: x => x.SchoolReportId,
                        principalTable: "SchoolReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Universities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    Levels = table.Column<int[]>(type: "integer[]", nullable: false),
                    FocusAreas = table.Column<int[]>(type: "integer[]", nullable: false),
                    UndergraduateCount = table.Column<int>(type: "integer", nullable: false),
                    GraduateCount = table.Column<int>(type: "integer", nullable: false),
                    AcceptanceRate = table.Column<double>(type: "double precision", nullable: false),
                    ResearchFunding = table.Column<int>(type: "integer", nullable: false),
                    HasStudentHousing = table.Column<bool>(type: "boolean", nullable: false),
                    Departments = table.Column<string[]>(type: "text[]", nullable: false),
                    UniversityReportId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Universities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Universities_EducationalInstitutions_Id",
                        column: x => x.Id,
                        principalTable: "EducationalInstitutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Universities_UniversityReports_UniversityReportId",
                        column: x => x.UniversityReportId,
                        principalTable: "UniversityReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schools_SchoolReportId",
                table: "Schools",
                column: "SchoolReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Universities_UniversityReportId",
                table: "Universities",
                column: "UniversityReportId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Faculties_Universities_UniversityId",
                table: "Faculties",
                column: "UniversityId",
                principalTable: "Universities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Schools_SchoolId",
                table: "Students",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Universities_UniversityId",
                table: "Students",
                column: "UniversityId",
                principalTable: "Universities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Faculties_Universities_UniversityId",
                table: "Faculties");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Schools_SchoolId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Universities_UniversityId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "Schools");

            migrationBuilder.DropTable(
                name: "Universities");

            migrationBuilder.AddColumn<double>(
                name: "AcceptanceRate",
                table: "EducationalInstitutions",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "Departments",
                table: "EducationalInstitutions",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtracurricularActivities",
                table: "EducationalInstitutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int[]>(
                name: "FocusAreas",
                table: "EducationalInstitutions",
                type: "integer[]",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GraduateCount",
                table: "EducationalInstitutions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasSpecialEducation",
                table: "EducationalInstitutions",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasStudentHousing",
                table: "EducationalInstitutions",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasUniform",
                table: "EducationalInstitutions",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstitutionType",
                table: "EducationalInstitutions",
                type: "character varying(34)",
                maxLength: 34,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "EducationalInstitutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int[]>(
                name: "Levels",
                table: "EducationalInstitutions",
                type: "integer[]",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "Programs",
                table: "EducationalInstitutions",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResearchFunding",
                table: "EducationalInstitutions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolReportId",
                table: "EducationalInstitutions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentCount",
                table: "EducationalInstitutions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "StudentTeacherRatio",
                table: "EducationalInstitutions",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UndergraduateCount",
                table: "EducationalInstitutions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UniversityReportId",
                table: "EducationalInstitutions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EducationalInstitutions_SchoolReportId",
                table: "EducationalInstitutions",
                column: "SchoolReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EducationalInstitutions_UniversityReportId",
                table: "EducationalInstitutions",
                column: "UniversityReportId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EducationalInstitutions_SchoolReports_SchoolReportId",
                table: "EducationalInstitutions",
                column: "SchoolReportId",
                principalTable: "SchoolReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EducationalInstitutions_UniversityReports_UniversityReportId",
                table: "EducationalInstitutions",
                column: "UniversityReportId",
                principalTable: "UniversityReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Faculties_EducationalInstitutions_UniversityId",
                table: "Faculties",
                column: "UniversityId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_EducationalInstitutions_SchoolId",
                table: "Students",
                column: "SchoolId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_EducationalInstitutions_UniversityId",
                table: "Students",
                column: "UniversityId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
