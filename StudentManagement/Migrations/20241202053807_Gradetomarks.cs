using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentManagement.Migrations
{
    /// <inheritdoc />
    public partial class Gradetomarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClassId",
                table: "MarksDetail",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "GradeId",
                table: "MarksDetail",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_MarksDetail_ClassId",
                table: "MarksDetail",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_MarksDetail_GradeId",
                table: "MarksDetail",
                column: "GradeId");

            migrationBuilder.AddForeignKey(
                name: "FK_MarksDetail_Class_ClassId",
                table: "MarksDetail",
                column: "ClassId",
                principalTable: "Class",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MarksDetail_Grades_GradeId",
                table: "MarksDetail",
                column: "GradeId",
                principalTable: "Grades",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MarksDetail_Class_ClassId",
                table: "MarksDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_MarksDetail_Grades_GradeId",
                table: "MarksDetail");

            migrationBuilder.DropIndex(
                name: "IX_MarksDetail_ClassId",
                table: "MarksDetail");

            migrationBuilder.DropIndex(
                name: "IX_MarksDetail_GradeId",
                table: "MarksDetail");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "MarksDetail");

            migrationBuilder.DropColumn(
                name: "GradeId",
                table: "MarksDetail");
        }
    }
}
