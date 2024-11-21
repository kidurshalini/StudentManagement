using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentManagement.Migrations
{
    /// <inheritdoc />
    public partial class addUseracadamic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClassRegistrationModelId",
                table: "Subject",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClassRegistrationModelId",
                table: "Class",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClassRegistrationModelId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserAcadamic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAcadamic", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAcadamic_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAcadamic_Class_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Class",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAcadamic_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subject_ClassRegistrationModelId",
                table: "Subject",
                column: "ClassRegistrationModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Class_ClassRegistrationModelId",
                table: "Class",
                column: "ClassRegistrationModelId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ClassRegistrationModelId",
                table: "AspNetUsers",
                column: "ClassRegistrationModelId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAcadamic_ClassId",
                table: "UserAcadamic",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAcadamic_GradeId",
                table: "UserAcadamic",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAcadamic_UserID",
                table: "UserAcadamic",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_UserAcadamic_ClassRegistrationModelId",
                table: "AspNetUsers",
                column: "ClassRegistrationModelId",
                principalTable: "UserAcadamic",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Class_UserAcadamic_ClassRegistrationModelId",
                table: "Class",
                column: "ClassRegistrationModelId",
                principalTable: "UserAcadamic",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subject_UserAcadamic_ClassRegistrationModelId",
                table: "Subject",
                column: "ClassRegistrationModelId",
                principalTable: "UserAcadamic",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_UserAcadamic_ClassRegistrationModelId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Class_UserAcadamic_ClassRegistrationModelId",
                table: "Class");

            migrationBuilder.DropForeignKey(
                name: "FK_Subject_UserAcadamic_ClassRegistrationModelId",
                table: "Subject");

            migrationBuilder.DropTable(
                name: "UserAcadamic");

            migrationBuilder.DropIndex(
                name: "IX_Subject_ClassRegistrationModelId",
                table: "Subject");

            migrationBuilder.DropIndex(
                name: "IX_Class_ClassRegistrationModelId",
                table: "Class");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ClassRegistrationModelId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ClassRegistrationModelId",
                table: "Subject");

            migrationBuilder.DropColumn(
                name: "ClassRegistrationModelId",
                table: "Class");

            migrationBuilder.DropColumn(
                name: "ClassRegistrationModelId",
                table: "AspNetUsers");
        }
    }
}
