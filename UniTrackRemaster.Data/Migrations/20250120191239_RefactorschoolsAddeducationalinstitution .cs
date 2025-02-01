using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorschoolsAddeducationalinstitution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Schools_SchoolId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Schools_SchoolId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Universities_UniversityId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Faculties_Universities_UniversityId",
                table: "Faculties");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Schools_SchoolId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Universities_UniversityId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Universities_UniversityId",
                table: "Teachers");

            migrationBuilder.DropForeignKey(
                name: "FK_Universities_UniversityReports_UniversityReportId",
                table: "Universities");

            migrationBuilder.DropTable(
                name: "SchoolImages");

            migrationBuilder.DropTable(
                name: "SchoolTeacher");

            migrationBuilder.DropTable(
                name: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Events_SchoolId",
                table: "Events");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Universities",
                table: "Universities");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "Events");

            migrationBuilder.RenameTable(
                name: "Universities",
                newName: "EducationalInstitutions");

            migrationBuilder.RenameColumn(
                name: "UniversityId",
                table: "Teachers",
                newName: "EducationalInstitutionId");

            migrationBuilder.RenameIndex(
                name: "IX_Teachers_UniversityId",
                table: "Teachers",
                newName: "IX_Teachers_EducationalInstitutionId");

            migrationBuilder.RenameColumn(
                name: "UniversityId",
                table: "Events",
                newName: "EducationalInstitutionId");

            migrationBuilder.RenameIndex(
                name: "IX_Events_UniversityId",
                table: "Events",
                newName: "IX_Events_EducationalInstitutionId");

            migrationBuilder.RenameIndex(
                name: "IX_Universities_UniversityReportId",
                table: "EducationalInstitutions",
                newName: "IX_EducationalInstitutions_UniversityReportId");

            migrationBuilder.AlterColumn<Guid>(
                name: "UniversityReportId",
                table: "EducationalInstitutions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<double>(
                name: "AcceptanceRate",
                table: "EducationalInstitutions",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Accreditations",
                table: "EducationalInstitutions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "AddressId",
                table: "EducationalInstitutions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string[]>(
                name: "Departments",
                table: "EducationalInstitutions",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "EducationalInstitutions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EstablishedDate",
                table: "EducationalInstitutions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

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

            migrationBuilder.AddColumn<int>(
                name: "IntegrationStatus",
                table: "EducationalInstitutions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "EducationalInstitutions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Motto",
                table: "EducationalInstitutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "EducationalInstitutions",
                type: "text",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddColumn<Guid>(
                name: "TeacherId",
                table: "EducationalInstitutions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "EducationalInstitutions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UndergraduateCount",
                table: "EducationalInstitutions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "EducationalInstitutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EducationalInstitutions",
                table: "EducationalInstitutions",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    Url = table.Column<string>(type: "text", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    UniversityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Images_EducationalInstitutions_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "EducationalInstitutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Images_EducationalInstitutions_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "EducationalInstitutions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EducationalInstitutions_AddressId",
                table: "EducationalInstitutions",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationalInstitutions_SchoolReportId",
                table: "EducationalInstitutions",
                column: "SchoolReportId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationalInstitutions_TeacherId",
                table: "EducationalInstitutions",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_SchoolId",
                table: "Images",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_UniversityId",
                table: "Images",
                column: "UniversityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_EducationalInstitutions_SchoolId",
                table: "Applications",
                column: "SchoolId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EducationalInstitutions_SchoolAddress_AddressId",
                table: "EducationalInstitutions",
                column: "AddressId",
                principalTable: "SchoolAddress",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EducationalInstitutions_SchoolReports_SchoolReportId",
                table: "EducationalInstitutions",
                column: "SchoolReportId",
                principalTable: "SchoolReports",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EducationalInstitutions_Teachers_TeacherId",
                table: "EducationalInstitutions",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EducationalInstitutions_UniversityReports_UniversityReportId",
                table: "EducationalInstitutions",
                column: "UniversityReportId",
                principalTable: "UniversityReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_EducationalInstitutions_EducationalInstitutionId",
                table: "Events",
                column: "EducationalInstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Faculties_EducationalInstitutions_UniversityId",
                table: "Faculties",
                column: "UniversityId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_EducationalInstitutions_SchoolId",
                table: "Students",
                column: "SchoolId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_EducationalInstitutions_UniversityId",
                table: "Students",
                column: "UniversityId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_EducationalInstitutions_EducationalInstitutionId",
                table: "Teachers",
                column: "EducationalInstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_EducationalInstitutions_SchoolId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_EducationalInstitutions_SchoolAddress_AddressId",
                table: "EducationalInstitutions");

            migrationBuilder.DropForeignKey(
                name: "FK_EducationalInstitutions_SchoolReports_SchoolReportId",
                table: "EducationalInstitutions");

            migrationBuilder.DropForeignKey(
                name: "FK_EducationalInstitutions_Teachers_TeacherId",
                table: "EducationalInstitutions");

            migrationBuilder.DropForeignKey(
                name: "FK_EducationalInstitutions_UniversityReports_UniversityReportId",
                table: "EducationalInstitutions");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_EducationalInstitutions_EducationalInstitutionId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Faculties_EducationalInstitutions_UniversityId",
                table: "Faculties");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_EducationalInstitutions_SchoolId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_EducationalInstitutions_UniversityId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_EducationalInstitutions_EducationalInstitutionId",
                table: "Teachers");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EducationalInstitutions",
                table: "EducationalInstitutions");

            migrationBuilder.DropIndex(
                name: "IX_EducationalInstitutions_AddressId",
                table: "EducationalInstitutions");

            migrationBuilder.DropIndex(
                name: "IX_EducationalInstitutions_SchoolReportId",
                table: "EducationalInstitutions");

            migrationBuilder.DropIndex(
                name: "IX_EducationalInstitutions_TeacherId",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "AcceptanceRate",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "Accreditations",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "Departments",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "EstablishedDate",
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
                name: "IntegrationStatus",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "Levels",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "Motto",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "Phone",
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
                name: "TeacherId",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "UndergraduateCount",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "EducationalInstitutions");

            migrationBuilder.RenameTable(
                name: "EducationalInstitutions",
                newName: "Universities");

            migrationBuilder.RenameColumn(
                name: "EducationalInstitutionId",
                table: "Teachers",
                newName: "UniversityId");

            migrationBuilder.RenameIndex(
                name: "IX_Teachers_EducationalInstitutionId",
                table: "Teachers",
                newName: "IX_Teachers_UniversityId");

            migrationBuilder.RenameColumn(
                name: "EducationalInstitutionId",
                table: "Events",
                newName: "UniversityId");

            migrationBuilder.RenameIndex(
                name: "IX_Events_EducationalInstitutionId",
                table: "Events",
                newName: "IX_Events_UniversityId");

            migrationBuilder.RenameIndex(
                name: "IX_EducationalInstitutions_UniversityReportId",
                table: "Universities",
                newName: "IX_Universities_UniversityReportId");

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "Events",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UniversityReportId",
                table: "Universities",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Universities",
                table: "Universities",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    AddressId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IntegrationStatus = table.Column<int>(type: "integer", nullable: false),
                    Motto = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Programs = table.Column<string[]>(type: "text[]", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Website = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schools_SchoolAddress_AddressId",
                        column: x => x.AddressId,
                        principalTable: "SchoolAddress",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Schools_SchoolReports_SchoolReportId",
                        column: x => x.SchoolReportId,
                        principalTable: "SchoolReports",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SchoolImages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolImages_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchoolTeacher",
                columns: table => new
                {
                    SchoolsId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeachersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolTeacher", x => new { x.SchoolsId, x.TeachersId });
                    table.ForeignKey(
                        name: "FK_SchoolTeacher_Schools_SchoolsId",
                        column: x => x.SchoolsId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SchoolTeacher_Teachers_TeachersId",
                        column: x => x.TeachersId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_SchoolId",
                table: "Events",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolImages_SchoolId",
                table: "SchoolImages",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolTeacher_TeachersId",
                table: "SchoolTeacher",
                column: "TeachersId");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_AddressId",
                table: "Schools",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_SchoolReportId",
                table: "Schools",
                column: "SchoolReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Schools_SchoolId",
                table: "Applications",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Schools_SchoolId",
                table: "Events",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Universities_UniversityId",
                table: "Events",
                column: "UniversityId",
                principalTable: "Universities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Faculties_Universities_UniversityId",
                table: "Faculties",
                column: "UniversityId",
                principalTable: "Universities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Schools_SchoolId",
                table: "Students",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Universities_UniversityId",
                table: "Students",
                column: "UniversityId",
                principalTable: "Universities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Universities_UniversityId",
                table: "Teachers",
                column: "UniversityId",
                principalTable: "Universities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Universities_UniversityReports_UniversityReportId",
                table: "Universities",
                column: "UniversityReportId",
                principalTable: "UniversityReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
