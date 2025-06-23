using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAbsenceRelationToSemester : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SemesterId",
                table: "Absences",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Absences_SemesterId",
                table: "Absences",
                column: "SemesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Semesters_SemesterId",
                table: "Absences",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Semesters_SemesterId",
                table: "Absences");

            migrationBuilder.DropIndex(
                name: "IX_Absences_SemesterId",
                table: "Absences");

            migrationBuilder.DropColumn(
                name: "SemesterId",
                table: "Absences");
        }
    }
}
