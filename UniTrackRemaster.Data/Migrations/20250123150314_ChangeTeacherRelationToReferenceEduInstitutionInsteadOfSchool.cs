using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTeacherRelationToReferenceEduInstitutionInsteadOfSchool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EducationalInstitutions_Teachers_TeacherId",
                table: "EducationalInstitutions");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_EducationalInstitutions_EducationalInstitutionId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_EducationalInstitutions_TeacherId",
                table: "EducationalInstitutions");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "EducationalInstitutions");

            migrationBuilder.AlterColumn<Guid>(
                name: "EducationalInstitutionId",
                table: "Teachers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Admins",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstitutionId",
                table: "Admins",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Admins",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Admins",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Admins",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Admins",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Admins",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AdminPermission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permission = table.Column<int>(type: "integer", nullable: false),
                    Scope = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminPermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminPermission_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admins_InstitutionId",
                table: "Admins",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminPermission_AdminId",
                table: "AdminPermission",
                column: "AdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_EducationalInstitutions_InstitutionId",
                table: "Admins",
                column: "InstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_EducationalInstitutions_EducationalInstitutionId",
                table: "Teachers",
                column: "EducationalInstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_EducationalInstitutions_InstitutionId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_EducationalInstitutions_EducationalInstitutionId",
                table: "Teachers");

            migrationBuilder.DropTable(
                name: "AdminPermission");

            migrationBuilder.DropIndex(
                name: "IX_Admins_InstitutionId",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Admins");

            migrationBuilder.AlterColumn<Guid>(
                name: "EducationalInstitutionId",
                table: "Teachers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "TeacherId",
                table: "EducationalInstitutions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EducationalInstitutions_TeacherId",
                table: "EducationalInstitutions",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_EducationalInstitutions_Teachers_TeacherId",
                table: "EducationalInstitutions",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_EducationalInstitutions_EducationalInstitutionId",
                table: "Teachers",
                column: "EducationalInstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id");
        }
    }
}
