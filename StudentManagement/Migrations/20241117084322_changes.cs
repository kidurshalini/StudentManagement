using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentManagement.Migrations
{
    /// <inheritdoc />
    public partial class changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GradeModelID",
                table: "Subject",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Class",
                table: "Class",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Subject_GradeModelID",
                table: "Subject",
                column: "GradeModelID");

            migrationBuilder.AddForeignKey(
                name: "FK_Subject_Grades_GradeModelID",
                table: "Subject",
                column: "GradeModelID",
                principalTable: "Grades",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subject_Grades_GradeModelID",
                table: "Subject");

            migrationBuilder.DropIndex(
                name: "IX_Subject_GradeModelID",
                table: "Subject");

            migrationBuilder.DropColumn(
                name: "GradeModelID",
                table: "Subject");

            migrationBuilder.AlterColumn<int>(
                name: "Class",
                table: "Class",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
