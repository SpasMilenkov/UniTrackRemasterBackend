using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEducationalInstitutionToInstitution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYears_EducationalInstitutions_EducationalInstitutio~",
                table: "AcademicYears");

            migrationBuilder.DropForeignKey(
                name: "FK_Admins_EducationalInstitutions_InstitutionId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_EducationalInstitutions_InstitutionId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Club_EducationalInstitutions_EducationalInstitutionId",
                table: "Club");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_EducationalInstitutions_EducationalInstitutionId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_EducationalInstitutions_InstitutionId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Schools_EducationalInstitutions_InstitutionId",
                table: "Schools");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_EducationalInstitutions_EducationalInstitutionId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_EducationalInstitutions_EducationalInstitutionId",
                table: "Teachers");

            migrationBuilder.DropForeignKey(
                name: "FK_Universities_EducationalInstitutions_InstitutionId",
                table: "Universities");

            migrationBuilder.DropTable(
                name: "EducationalInstitutions");

            migrationBuilder.RenameColumn(
                name: "EducationalInstitutionId",
                table: "Teachers",
                newName: "InstitutionId");

            migrationBuilder.RenameIndex(
                name: "IX_Teachers_EducationalInstitutionId",
                table: "Teachers",
                newName: "IX_Teachers_InstitutionId");

            migrationBuilder.RenameColumn(
                name: "EducationalInstitutionId",
                table: "Students",
                newName: "InstitutionId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_EducationalInstitutionId",
                table: "Students",
                newName: "IX_Students_InstitutionId");

            migrationBuilder.RenameColumn(
                name: "EducationalInstitutionId",
                table: "Events",
                newName: "InstitutionId");

            migrationBuilder.RenameIndex(
                name: "IX_Events_EducationalInstitutionId",
                table: "Events",
                newName: "IX_Events_InstitutionId");

            migrationBuilder.RenameColumn(
                name: "EducationalInstitutionId",
                table: "Club",
                newName: "InstitutionId");

            migrationBuilder.RenameIndex(
                name: "IX_Club_EducationalInstitutionId",
                table: "Club",
                newName: "IX_Club_InstitutionId");

            migrationBuilder.RenameColumn(
                name: "EducationalInstitutionId",
                table: "AcademicYears",
                newName: "InstitutionId");

            migrationBuilder.RenameIndex(
                name: "IX_AcademicYears_EducationalInstitutionId",
                table: "AcademicYears",
                newName: "IX_AcademicYears_InstitutionId");

            migrationBuilder.CreateTable(
                name: "Institutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    Accreditations = table.Column<string>(type: "text", nullable: false),
                    AddressId = table.Column<Guid>(type: "uuid", nullable: false),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    EstablishedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Website = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Motto = table.Column<string>(type: "text", nullable: true),
                    IntegrationStatus = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Institutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Institutions_SchoolAddress_AddressId",
                        column: x => x.AddressId,
                        principalTable: "SchoolAddress",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Institutions_AddressId",
                table: "Institutions",
                column: "AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYears_Institutions_InstitutionId",
                table: "AcademicYears",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_Institutions_InstitutionId",
                table: "Admins",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Institutions_InstitutionId",
                table: "Applications",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Club_Institutions_InstitutionId",
                table: "Club",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Institutions_InstitutionId",
                table: "Events",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Institutions_InstitutionId",
                table: "Images",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_Institutions_InstitutionId",
                table: "Schools",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Institutions_InstitutionId",
                table: "Students",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Institutions_InstitutionId",
                table: "Teachers",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Universities_Institutions_InstitutionId",
                table: "Universities",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYears_Institutions_InstitutionId",
                table: "AcademicYears");

            migrationBuilder.DropForeignKey(
                name: "FK_Admins_Institutions_InstitutionId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Institutions_InstitutionId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Club_Institutions_InstitutionId",
                table: "Club");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Institutions_InstitutionId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Institutions_InstitutionId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Schools_Institutions_InstitutionId",
                table: "Schools");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Institutions_InstitutionId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Institutions_InstitutionId",
                table: "Teachers");

            migrationBuilder.DropForeignKey(
                name: "FK_Universities_Institutions_InstitutionId",
                table: "Universities");

            migrationBuilder.DropTable(
                name: "Institutions");

            migrationBuilder.RenameColumn(
                name: "InstitutionId",
                table: "Teachers",
                newName: "EducationalInstitutionId");

            migrationBuilder.RenameIndex(
                name: "IX_Teachers_InstitutionId",
                table: "Teachers",
                newName: "IX_Teachers_EducationalInstitutionId");

            migrationBuilder.RenameColumn(
                name: "InstitutionId",
                table: "Students",
                newName: "EducationalInstitutionId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_InstitutionId",
                table: "Students",
                newName: "IX_Students_EducationalInstitutionId");

            migrationBuilder.RenameColumn(
                name: "InstitutionId",
                table: "Events",
                newName: "EducationalInstitutionId");

            migrationBuilder.RenameIndex(
                name: "IX_Events_InstitutionId",
                table: "Events",
                newName: "IX_Events_EducationalInstitutionId");

            migrationBuilder.RenameColumn(
                name: "InstitutionId",
                table: "Club",
                newName: "EducationalInstitutionId");

            migrationBuilder.RenameIndex(
                name: "IX_Club_InstitutionId",
                table: "Club",
                newName: "IX_Club_EducationalInstitutionId");

            migrationBuilder.RenameColumn(
                name: "InstitutionId",
                table: "AcademicYears",
                newName: "EducationalInstitutionId");

            migrationBuilder.RenameIndex(
                name: "IX_AcademicYears_InstitutionId",
                table: "AcademicYears",
                newName: "IX_AcademicYears_EducationalInstitutionId");

            migrationBuilder.CreateTable(
                name: "EducationalInstitutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    AddressId = table.Column<Guid>(type: "uuid", nullable: false),
                    Accreditations = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: false),
                    EstablishedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IntegrationStatus = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    Motto = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Website = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationalInstitutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EducationalInstitutions_SchoolAddress_AddressId",
                        column: x => x.AddressId,
                        principalTable: "SchoolAddress",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EducationalInstitutions_AddressId",
                table: "EducationalInstitutions",
                column: "AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYears_EducationalInstitutions_EducationalInstitutio~",
                table: "AcademicYears",
                column: "EducationalInstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_EducationalInstitutions_InstitutionId",
                table: "Admins",
                column: "InstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_EducationalInstitutions_InstitutionId",
                table: "Applications",
                column: "InstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Club_EducationalInstitutions_EducationalInstitutionId",
                table: "Club",
                column: "EducationalInstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_EducationalInstitutions_EducationalInstitutionId",
                table: "Events",
                column: "EducationalInstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_EducationalInstitutions_InstitutionId",
                table: "Images",
                column: "InstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_EducationalInstitutions_InstitutionId",
                table: "Schools",
                column: "InstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_EducationalInstitutions_EducationalInstitutionId",
                table: "Students",
                column: "EducationalInstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_EducationalInstitutions_EducationalInstitutionId",
                table: "Teachers",
                column: "EducationalInstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Universities_EducationalInstitutions_InstitutionId",
                table: "Universities",
                column: "InstitutionId",
                principalTable: "EducationalInstitutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
