using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skill_Loop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HardenRefreshTokenStorage : Migration
    {
        /// <summary>
        /// SECURITY MIGRATION — read before deploying.
        ///
        /// This changes how refresh tokens are persisted: the plaintext <c>Token</c> column is
        /// replaced by a SHA-256 <c>TokenHash</c>, and rotation/reuse detection is added via
        /// <c>TokenFamilyId</c> and <c>ReplacedByTokenHash</c>.
        ///
        /// OPERATIONAL IMPACT: every existing session is invalidated and all users must sign
        /// in again. This is unavoidable and intended. The old column held the bearer token
        /// itself, so a digest cannot be back-filled from it without reading the plaintext we
        /// are removing. Rotating rather than migrating is the correct trade-off: those
        /// sessions are assumed compromised precisely because the plaintext was stored.
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens");

            // Delete every existing row before the new unique index on TokenHash is created.
            //
            // The scaffolded migration would have left existing rows behind with
            // TokenHash = "" (the non-nullable column default). Any table holding two or
            // more refresh tokens would then fail the unique index, so the migration would
            // abort partway and leave the schema half-migrated. Deleting first also matches
            // the intent: those tokens cannot be carried into the hashed model.
            migrationBuilder.Sql("DELETE FROM [RefreshTokens];");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "RefreshTokens");

            migrationBuilder.AddColumn<string>(
                name: "ReplacedByTokenHash",
                table: "RefreshTokens",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TokenFamilyId",
                table: "RefreshTokens",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                table: "RefreshTokens",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            // No backfill for TokenHash or TokenFamilyId: the table is empty at this point
            // (see the DELETE above), so there is nothing to carry over. New rows get both
            // values from RefreshTokenService.SaveTokenAsync, which assigns a fresh
            // Guid.NewGuid() family per login — never Guid.Empty, which would collapse every
            // user into one family and turn a single replayed token into a system-wide
            // logout.

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenFamilyId",
                table: "RefreshTokens",
                column: "TokenFamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rolling back re-introduces plaintext token storage, so it is not a supported
            // operation. Left in place only because EF requires a Down, but note that the
            // restored schema cannot migrate the existing rows: their digests are not
            // reversible. A rollback therefore also requires every user to sign in again.
            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_TokenFamilyId",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "ReplacedByTokenHash",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "TokenFamilyId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "TokenHash",
                table: "RefreshTokens");

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "RefreshTokens",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);
        }
    }
}
