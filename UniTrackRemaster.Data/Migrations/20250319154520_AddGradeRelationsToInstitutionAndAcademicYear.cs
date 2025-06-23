using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeRelationsToInstitutionAndAcademicYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HomeRoomTeacher_AcademicYears_AcademicYearId",
                table: "HomeRoomTeacher");

            migrationBuilder.DropIndex(
                name: "IX_HomeRoomTeacher_AcademicYearId",
                table: "HomeRoomTeacher");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "HomeRoomTeacher");

            migrationBuilder.DropColumn(
                name: "Responsibilities",
                table: "HomeRoomTeacher");

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId",
                table: "Grades",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "InstitutionId",
                table: "Grades",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Grades_AcademicYearId",
                table: "Grades",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_InstitutionId",
                table: "Grades",
                column: "InstitutionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_AcademicYears_AcademicYearId",
                table: "Grades",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_Institutions_InstitutionId",
                table: "Grades",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grades_AcademicYears_AcademicYearId",
                table: "Grades");

            migrationBuilder.DropForeignKey(
                name: "FK_Grades_Institutions_InstitutionId",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_Grades_AcademicYearId",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_Grades_InstitutionId",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "Grades");

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId",
                table: "HomeRoomTeacher",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Responsibilities",
                table: "HomeRoomTeacher",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_HomeRoomTeacher_AcademicYearId",
                table: "HomeRoomTeacher",
                column: "AcademicYearId");

            migrationBuilder.AddForeignKey(
                name: "FK_HomeRoomTeacher_AcademicYears_AcademicYearId",
                table: "HomeRoomTeacher",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
