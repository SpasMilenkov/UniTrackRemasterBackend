using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolAddressSchoolImagesFixSchoolRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Majors_Schools_SchoolId",
                table: "Majors");

            migrationBuilder.DropIndex(
                name: "IX_Majors_SchoolId",
                table: "Majors");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "Majors");

            migrationBuilder.AddColumn<Guid>(
                name: "AddressId",
                table: "Schools",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "IntegrationStatus",
                table: "Schools",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Moto",
                table: "Schools",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string[]>(
                name: "Programs",
                table: "Schools",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Schools",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchoolAddress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    Country = table.Column<string>(type: "text", nullable: false),
                    Settlement = table.Column<string>(type: "text", nullable: false),
                    PostalCode = table.Column<string>(type: "text", nullable: false),
                    Street = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolAddress", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchoolImages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolImage_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schools_AddressId",
                table: "Schools",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_SchoolId",
                table: "Applications",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolImages_SchoolId",
                table: "SchoolImages",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_SchoolAddress_AddressId",
                table: "Schools",
                column: "AddressId",
                principalTable: "SchoolAddress",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schools_SchoolAddress_AddressId",
                table: "Schools");

            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "SchoolAddress");

            migrationBuilder.DropTable(
                name: "SchoolImages");

            migrationBuilder.DropIndex(
                name: "IX_Schools_AddressId",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "IntegrationStatus",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Moto",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Programs",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Schools");

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "Majors",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Majors_SchoolId",
                table: "Majors",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Majors_Schools_SchoolId",
                table: "Majors",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id");
        }
    }
}
