using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentManagement.Migrations
{
    /// <inheritdoc />
    public partial class updatemigrtation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Class_Grade_GradeId",
                table: "Class");

            migrationBuilder.DropForeignKey(
                name: "FK_Subject_Grade_GradeId",
                table: "Subject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Grade",
                table: "Grade");

            migrationBuilder.RenameTable(
                name: "Grade",
                newName: "Grades");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Grades",
                table: "Grades",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Class_Grades_GradeId",
                table: "Class",
                column: "GradeId",
                principalTable: "Grades",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subject_Grades_GradeId",
                table: "Subject",
                column: "GradeId",
                principalTable: "Grades",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Class_Grades_GradeId",
                table: "Class");

            migrationBuilder.DropForeignKey(
                name: "FK_Subject_Grades_GradeId",
                table: "Subject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Grades",
                table: "Grades");

            migrationBuilder.RenameTable(
                name: "Grades",
                newName: "Grade");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Grade",
                table: "Grade",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Class_Grade_GradeId",
                table: "Class",
                column: "GradeId",
                principalTable: "Grade",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subject_Grade_GradeId",
                table: "Subject",
                column: "GradeId",
                principalTable: "Grade",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
