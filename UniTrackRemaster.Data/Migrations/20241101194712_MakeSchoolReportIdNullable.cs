using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeSchoolReportIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schools_SchoolReports_SchoolReportId",
                table: "Schools");

            migrationBuilder.AlterColumn<Guid>(
                name: "SchoolReportId",
                table: "Schools",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_SchoolReports_SchoolReportId",
                table: "Schools",
                column: "SchoolReportId",
                principalTable: "SchoolReports",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schools_SchoolReports_SchoolReportId",
                table: "Schools");

            migrationBuilder.AlterColumn<Guid>(
                name: "SchoolReportId",
                table: "Schools",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_SchoolReports_SchoolReportId",
                table: "Schools",
                column: "SchoolReportId",
                principalTable: "SchoolReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
