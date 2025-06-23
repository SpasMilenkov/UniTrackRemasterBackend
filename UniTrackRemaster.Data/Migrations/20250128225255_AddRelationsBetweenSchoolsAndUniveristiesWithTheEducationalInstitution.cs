using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationsBetweenSchoolsAndUniveristiesWithTheEducationalInstitution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Schools_SchoolId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Universities_UniversityId",
                table: "Students");

            migrationBuilder.AddColumn<Guid>(
                name: "InstitutionId",
                table: "Universities",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "EducationalInstitutionId",
                table: "Students",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstitutionId",
                table: "Schools",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Universities_InstitutionId",
                table: "Universities",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_EducationalInstitutionId",
                table: "Students",
                column: "EducationalInstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_InstitutionId",
                table: "Schools",
                column: "InstitutionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_EducationalInstitutions_InstitutionId",
                table: "Schools",
                column: "InstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_EducationalInstitutions_EducationalInstitutionId",
                table: "Students",
                column: "EducationalInstitutionId",
                principalTable: "EducationalInstitutions",
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
                name: "FK_Universities_EducationalInstitutions_InstitutionId",
                table: "Universities",
                column: "InstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schools_EducationalInstitutions_InstitutionId",
                table: "Schools");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_EducationalInstitutions_EducationalInstitutionId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Schools_SchoolId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Universities_UniversityId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Universities_EducationalInstitutions_InstitutionId",
                table: "Universities");

            migrationBuilder.DropIndex(
                name: "IX_Universities_InstitutionId",
                table: "Universities");

            migrationBuilder.DropIndex(
                name: "IX_Students_EducationalInstitutionId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Schools_InstitutionId",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "Universities");

            migrationBuilder.DropColumn(
                name: "EducationalInstitutionId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "Schools");

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
    }
}
