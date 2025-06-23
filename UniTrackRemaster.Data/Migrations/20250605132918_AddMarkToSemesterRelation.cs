using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMarkToSemesterRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SemesterId",
                table: "Marks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Marks_SemesterId",
                table: "Marks",
                column: "SemesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Marks_Semesters_SemesterId",
                table: "Marks",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Marks_Semesters_SemesterId",
                table: "Marks");

            migrationBuilder.DropIndex(
                name: "IX_Marks_SemesterId",
                table: "Marks");

            migrationBuilder.DropColumn(
                name: "SemesterId",
                table: "Marks");
        }
    }
}
