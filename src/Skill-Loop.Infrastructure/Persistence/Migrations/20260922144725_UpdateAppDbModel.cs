using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skill_Loop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppDbModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "OtpVerifications",
                newName: "Identifier");

            migrationBuilder.RenameIndex(
                name: "IX_OtpVerifications_Phone_Purpose_IsConsumed",
                table: "OtpVerifications",
                newName: "IX_OtpVerifications_Identifier_Purpose_IsConsumed");

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupportEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactPhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FacebookUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InstagramUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhatsAppNumber = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.RenameColumn(
                name: "Identifier",
                table: "OtpVerifications",
                newName: "Phone");

            migrationBuilder.RenameIndex(
                name: "IX_OtpVerifications_Identifier_Purpose_IsConsumed",
                table: "OtpVerifications",
                newName: "IX_OtpVerifications_Phone_Purpose_IsConsumed");
        }
    }
}
