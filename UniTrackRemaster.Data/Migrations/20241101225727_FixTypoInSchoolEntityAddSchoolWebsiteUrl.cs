using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixTypoInSchoolEntityAddSchoolWebsiteUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Moto",
                table: "Schools",
                newName: "Website");

            migrationBuilder.AddColumn<string>(
                name: "Motto",
                table: "Schools",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Motto",
                table: "Schools");

            migrationBuilder.RenameColumn(
                name: "Website",
                table: "Schools",
                newName: "Moto");
        }
    }
}
