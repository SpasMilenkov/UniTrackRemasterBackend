using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAttendanceToAbsence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Students_StudentId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Subjects_SubjectId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_ElectiveSubject_Grades_GradeId",
                table: "ElectiveSubject");

            migrationBuilder.DropForeignKey(
                name: "FK_ElectiveSubject_Teachers_TeacherId",
                table: "ElectiveSubject");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentElectives_ElectiveSubject_ElectiveSubjectId",
                table: "StudentElectives");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ElectiveSubject",
                table: "ElectiveSubject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Attendances",
                table: "Attendances");

            migrationBuilder.RenameTable(
                name: "ElectiveSubject",
                newName: "ElectiveSubjects");

            migrationBuilder.RenameTable(
                name: "Attendances",
                newName: "Absences");

            migrationBuilder.RenameIndex(
                name: "IX_ElectiveSubject_TeacherId",
                table: "ElectiveSubjects",
                newName: "IX_ElectiveSubjects_TeacherId");

            migrationBuilder.RenameIndex(
                name: "IX_ElectiveSubject_GradeId",
                table: "ElectiveSubjects",
                newName: "IX_ElectiveSubjects_GradeId");

            migrationBuilder.RenameIndex(
                name: "IX_Attendances_SubjectId",
                table: "Absences",
                newName: "IX_Absences_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Attendances_StudentId",
                table: "Absences",
                newName: "IX_Absences_StudentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ElectiveSubjects",
                table: "ElectiveSubjects",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Absences",
                table: "Absences",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Students_StudentId",
                table: "Absences",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Subjects_SubjectId",
                table: "Absences",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ElectiveSubjects_Grades_GradeId",
                table: "ElectiveSubjects",
                column: "GradeId",
                principalTable: "Grades",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ElectiveSubjects_Teachers_TeacherId",
                table: "ElectiveSubjects",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentElectives_ElectiveSubjects_ElectiveSubjectId",
                table: "StudentElectives",
                column: "ElectiveSubjectId",
                principalTable: "ElectiveSubjects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Students_StudentId",
                table: "Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Subjects_SubjectId",
                table: "Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_ElectiveSubjects_Grades_GradeId",
                table: "ElectiveSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_ElectiveSubjects_Teachers_TeacherId",
                table: "ElectiveSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentElectives_ElectiveSubjects_ElectiveSubjectId",
                table: "StudentElectives");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ElectiveSubjects",
                table: "ElectiveSubjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Absences",
                table: "Absences");

            migrationBuilder.RenameTable(
                name: "ElectiveSubjects",
                newName: "ElectiveSubject");

            migrationBuilder.RenameTable(
                name: "Absences",
                newName: "Attendances");

            migrationBuilder.RenameIndex(
                name: "IX_ElectiveSubjects_TeacherId",
                table: "ElectiveSubject",
                newName: "IX_ElectiveSubject_TeacherId");

            migrationBuilder.RenameIndex(
                name: "IX_ElectiveSubjects_GradeId",
                table: "ElectiveSubject",
                newName: "IX_ElectiveSubject_GradeId");

            migrationBuilder.RenameIndex(
                name: "IX_Absences_SubjectId",
                table: "Attendances",
                newName: "IX_Attendances_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Absences_StudentId",
                table: "Attendances",
                newName: "IX_Attendances_StudentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ElectiveSubject",
                table: "ElectiveSubject",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attendances",
                table: "Attendances",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Students_StudentId",
                table: "Attendances",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Subjects_SubjectId",
                table: "Attendances",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ElectiveSubject_Grades_GradeId",
                table: "ElectiveSubject",
                column: "GradeId",
                principalTable: "Grades",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ElectiveSubject_Teachers_TeacherId",
                table: "ElectiveSubject",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentElectives_ElectiveSubject_ElectiveSubjectId",
                table: "StudentElectives",
                column: "ElectiveSubjectId",
                principalTable: "ElectiveSubject",
                principalColumn: "Id");
        }
    }
}
