using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skill_Loop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Update_CourseBookmark_SoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CourseBookmarks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "CourseBookmarks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CourseBookmarks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CourseBookmarks");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CourseBookmarks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CourseBookmarks");
        }
    }
}
