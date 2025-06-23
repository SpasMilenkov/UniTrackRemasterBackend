using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniTrackRemaster.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeToHomeRoomTeacherRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HomeRoomTeacher_Grades_GradeId",
                table: "HomeRoomTeacher");

            migrationBuilder.DropForeignKey(
                name: "FK_HomeRoomTeacher_Teachers_TeacherId",
                table: "HomeRoomTeacher");

            migrationBuilder.DropIndex(
                name: "IX_HomeRoomTeacher_GradeId",
                table: "HomeRoomTeacher");

            migrationBuilder.AddColumn<Guid>(
                name: "HomeRoomTeacherId",
                table: "Grades",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_HomeRoomTeacher_GradeId",
                table: "HomeRoomTeacher",
                column: "GradeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_HomeRoomTeacher_Grades_GradeId",
                table: "HomeRoomTeacher",
                column: "GradeId",
                principalTable: "Grades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HomeRoomTeacher_Teachers_TeacherId",
                table: "HomeRoomTeacher",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HomeRoomTeacher_Grades_GradeId",
                table: "HomeRoomTeacher");

            migrationBuilder.DropForeignKey(
                name: "FK_HomeRoomTeacher_Teachers_TeacherId",
                table: "HomeRoomTeacher");

            migrationBuilder.DropIndex(
                name: "IX_HomeRoomTeacher_GradeId",
                table: "HomeRoomTeacher");

            migrationBuilder.DropColumn(
                name: "HomeRoomTeacherId",
                table: "Grades");

            migrationBuilder.CreateIndex(
                name: "IX_HomeRoomTeacher_GradeId",
                table: "HomeRoomTeacher",
                column: "GradeId");

            migrationBuilder.AddForeignKey(
                name: "FK_HomeRoomTeacher_Grades_GradeId",
                table: "HomeRoomTeacher",
                column: "GradeId",
                principalTable: "Grades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HomeRoomTeacher_Teachers_TeacherId",
                table: "HomeRoomTeacher",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
