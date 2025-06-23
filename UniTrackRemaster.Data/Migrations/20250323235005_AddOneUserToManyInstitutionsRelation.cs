using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOneUserToManyInstitutionsRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationUserId",
                table: "Institutions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Institutions_ApplicationUserId",
                table: "Institutions",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Institutions_AspNetUsers_ApplicationUserId",
                table: "Institutions",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Institutions_AspNetUsers_ApplicationUserId",
                table: "Institutions");

            migrationBuilder.DropIndex(
                name: "IX_Institutions_ApplicationUserId",
                table: "Institutions");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Institutions");
        }
    }
}
