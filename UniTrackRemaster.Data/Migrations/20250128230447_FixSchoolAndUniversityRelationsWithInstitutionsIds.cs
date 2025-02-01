using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixSchoolAndUniversityRelationsWithInstitutionsIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schools_EducationalInstitutions_Id",
                table: "Schools");

            migrationBuilder.DropForeignKey(
                name: "FK_Universities_EducationalInstitutions_Id",
                table: "Universities");

            migrationBuilder.DropIndex(
                name: "IX_Universities_InstitutionId",
                table: "Universities");

            migrationBuilder.DropIndex(
                name: "IX_Schools_InstitutionId",
                table: "Schools");

            migrationBuilder.CreateIndex(
                name: "IX_Universities_InstitutionId",
                table: "Universities",
                column: "InstitutionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schools_InstitutionId",
                table: "Schools",
                column: "InstitutionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Universities_InstitutionId",
                table: "Universities");

            migrationBuilder.DropIndex(
                name: "IX_Schools_InstitutionId",
                table: "Schools");

            migrationBuilder.CreateIndex(
                name: "IX_Universities_InstitutionId",
                table: "Universities",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_InstitutionId",
                table: "Schools",
                column: "InstitutionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_EducationalInstitutions_Id",
                table: "Schools",
                column: "Id",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Universities_EducationalInstitutions_Id",
                table: "Universities",
                column: "Id",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
