using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectInstitutionRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InstitutionId",
                table: "Subjects",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_InstitutionId",
                table: "Subjects",
                column: "InstitutionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Institutions_InstitutionId",
                table: "Subjects",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Institutions_InstitutionId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_InstitutionId",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "Subjects");
        }
    }
}
