using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExpandOnSubjectsAndElectives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentElective_ElectiveSubject_ElectiveSubjectId",
                table: "StudentElective");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentElective_Students_StudentId",
                table: "StudentElective");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentElective",
                table: "StudentElective");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "StudentElective");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StudentElective");

            migrationBuilder.RenameTable(
                name: "StudentElective",
                newName: "StudentElectives");

            migrationBuilder.RenameIndex(
                name: "IX_StudentElective_StudentId",
                table: "StudentElectives",
                newName: "IX_StudentElectives_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentElective_ElectiveSubjectId",
                table: "StudentElectives",
                newName: "IX_StudentElectives_ElectiveSubjectId");

            migrationBuilder.AddColumn<int>(
                name: "AcademicLevel",
                table: "Subjects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Subjects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CreditHours",
                table: "Subjects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditValue",
                table: "Subjects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "Subjects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ElectiveType",
                table: "Subjects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasLab",
                table: "Subjects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsElective",
                table: "Subjects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxGradeLevel",
                table: "Subjects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxStudents",
                table: "Subjects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinGradeLevel",
                table: "Subjects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryTeacherId",
                table: "Subjects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubjectType",
                table: "Subjects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "ElectiveSubjectId",
                table: "StudentElectives",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "SubjectId",
                table: "StudentElectives",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentElectives",
                table: "StudentElectives",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_DepartmentId",
                table: "Subjects",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_PrimaryTeacherId",
                table: "Subjects",
                column: "PrimaryTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentElectives_SubjectId",
                table: "StudentElectives",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentElectives_ElectiveSubject_ElectiveSubjectId",
                table: "StudentElectives",
                column: "ElectiveSubjectId",
                principalTable: "ElectiveSubject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentElectives_Students_StudentId",
                table: "StudentElectives",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentElectives_Subjects_SubjectId",
                table: "StudentElectives",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Departments_DepartmentId",
                table: "Subjects",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Teachers_PrimaryTeacherId",
                table: "Subjects",
                column: "PrimaryTeacherId",
                principalTable: "Teachers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentElectives_ElectiveSubject_ElectiveSubjectId",
                table: "StudentElectives");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentElectives_Students_StudentId",
                table: "StudentElectives");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentElectives_Subjects_SubjectId",
                table: "StudentElectives");

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Departments_DepartmentId",
                table: "Subjects");

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Teachers_PrimaryTeacherId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_DepartmentId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_PrimaryTeacherId",
                table: "Subjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentElectives",
                table: "StudentElectives");

            migrationBuilder.DropIndex(
                name: "IX_StudentElectives_SubjectId",
                table: "StudentElectives");

            migrationBuilder.DropColumn(
                name: "AcademicLevel",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "CreditHours",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "CreditValue",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "ElectiveType",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "HasLab",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "IsElective",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "MaxGradeLevel",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "MaxStudents",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "MinGradeLevel",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "PrimaryTeacherId",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "SubjectType",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "StudentElectives");

            migrationBuilder.RenameTable(
                name: "StudentElectives",
                newName: "StudentElective");

            migrationBuilder.RenameIndex(
                name: "IX_StudentElectives_StudentId",
                table: "StudentElective",
                newName: "IX_StudentElective_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentElectives_ElectiveSubjectId",
                table: "StudentElective",
                newName: "IX_StudentElective_ElectiveSubjectId");

            migrationBuilder.AlterColumn<Guid>(
                name: "ElectiveSubjectId",
                table: "StudentElective",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "StudentElective",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "StudentElective",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentElective",
                table: "StudentElective",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentElective_ElectiveSubject_ElectiveSubjectId",
                table: "StudentElective",
                column: "ElectiveSubjectId",
                principalTable: "ElectiveSubject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentElective_Students_StudentId",
                table: "StudentElective",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
