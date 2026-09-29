using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skill_Loop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAskedByUserIdToSupportQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AskedByUserId",
                table: "SupportQuestions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportQuestions_AskedByUserId",
                table: "SupportQuestions",
                column: "AskedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SupportQuestions_AskedByUserId",
                table: "SupportQuestions");

            migrationBuilder.DropColumn(
                name: "AskedByUserId",
                table: "SupportQuestions");
        }
    }
}
