using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentManagement.Migrations
{
    /// <inheritdoc />
    public partial class marksdetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Class_Grades_GradeId",
                table: "Class");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Class",
                newName: "Id");

            migrationBuilder.CreateTable(
                name: "MarksDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SubjectID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Marks = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Term = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarksDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarksDetail_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarksDetail_Subject_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subject",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarksDetail_SubjectID",
                table: "MarksDetail",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_MarksDetail_UserID",
                table: "MarksDetail",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Class_Grades_GradeId",
                table: "Class",
                column: "GradeId",
                principalTable: "Grades",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Class_Grades_GradeId",
                table: "Class");

            migrationBuilder.DropTable(
                name: "MarksDetail");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Class",
                newName: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Class_Grades_GradeId",
                table: "Class",
                column: "GradeId",
                principalTable: "Grades",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
