using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInstitutionAccountStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Status column to Students table
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Students",
                type: "text",
                nullable: false,
                defaultValue: "Active");

            // Add Status column to Teachers table
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Teachers",
                type: "text",
                nullable: false,
                defaultValue: "Active");

            // Add Status column to Admins table
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Admins",
                type: "text",
                nullable: false,
                defaultValue: "Active",
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Status", table: "Students");
            migrationBuilder.DropColumn(name: "Status", table: "Teachers");
            migrationBuilder.DropColumn(name: "Status", table: "Admins");
        }
    }
}
