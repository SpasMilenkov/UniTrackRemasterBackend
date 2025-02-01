using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class MovefromSchoolscopedpropertiestoEducationalInstituion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_EducationalInstitutions_SchoolId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_EducationalInstitutions_SchoolId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_EducationalInstitutions_UniversityId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_UniversityId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "UniversityId",
                table: "Images");

            migrationBuilder.RenameColumn(
                name: "SchoolId",
                table: "Images",
                newName: "InstitutionId");

            migrationBuilder.RenameIndex(
                name: "IX_Images_SchoolId",
                table: "Images",
                newName: "IX_Images_InstitutionId");

            migrationBuilder.RenameColumn(
                name: "SchoolId",
                table: "Applications",
                newName: "InstitutionId");

            migrationBuilder.RenameIndex(
                name: "IX_Applications_SchoolId",
                table: "Applications",
                newName: "IX_Applications_InstitutionId");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Images",
                type: "uuid using \"Id\"::uuid",
                nullable: false,
                defaultValueSql: "uuid_generate_v4()",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValueSql: "uuid_generate_v4()");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Applications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_EducationalInstitutions_InstitutionId",
                table: "Applications",
                column: "InstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_EducationalInstitutions_InstitutionId",
                table: "Images",
                column: "InstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_EducationalInstitutions_InstitutionId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_EducationalInstitutions_InstitutionId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Applications");

            migrationBuilder.RenameColumn(
                name: "InstitutionId",
                table: "Images",
                newName: "SchoolId");

            migrationBuilder.RenameIndex(
                name: "IX_Images_InstitutionId",
                table: "Images",
                newName: "IX_Images_SchoolId");

            migrationBuilder.RenameColumn(
                name: "InstitutionId",
                table: "Applications",
                newName: "SchoolId");

            migrationBuilder.RenameIndex(
                name: "IX_Applications_InstitutionId",
                table: "Applications",
                newName: "IX_Applications_SchoolId");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Images",
                type: "text",
                nullable: false,
                defaultValueSql: "uuid_generate_v4()",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuid_generate_v4()");

            migrationBuilder.AddColumn<Guid>(
                name: "UniversityId",
                table: "Images",
                type: "uuid",
                nullable: true);

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
                name: "FK_Images_EducationalInstitutions_SchoolId",
                table: "Images",
                column: "SchoolId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_EducationalInstitutions_UniversityId",
                table: "Images",
                column: "UniversityId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id");
        }
    }
}
