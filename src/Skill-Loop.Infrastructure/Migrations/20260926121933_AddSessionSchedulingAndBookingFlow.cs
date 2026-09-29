using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skill_Loop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionSchedulingAndBookingFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Sessions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "CreditsPrice",
                table: "Sessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Sessions",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Sessions",
                type: "int",
                nullable: false,
                defaultValue: 60);

            migrationBuilder.AddColumn<string>(
                name: "LocationDetails",
                table: "Sessions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationType",
                table: "Sessions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Online");

            migrationBuilder.AddColumn<int>(
                name: "MaxParticipants",
                table: "Sessions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledAtUtc",
                table: "Sessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BookedAtUtc",
                table: "Bookings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Bookings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAtUtc",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAtUtc",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Bookings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Bookings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriceInCredits",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledAtUtc",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAtUtc",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Bookings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ScheduledAtUtc",
                table: "Sessions",
                column: "ScheduledAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_Status_ScheduledAtUtc",
                table: "Sessions",
                columns: new[] { "Status", "ScheduledAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SessionId_LearnerUserId",
                table: "Bookings",
                columns: new[] { "SessionId", "LearnerUserId" },
                unique: true,
                filter: "[Status] IN ('Pending', 'Confirmed', 'InProgress', 'Completed')");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SessionId_LearnerUserId_Status",
                table: "Bookings",
                columns: new[] { "SessionId", "LearnerUserId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Sessions_SessionId",
                table: "Bookings",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // الحجوزات القديمة كانت من غير تاريخ — نرجّعله تاريخ الإنشاء الحالي
            // (مش بنلفتر على Sessions لأن الجلسة من غير موعد لازم تفضل غير قابلة للحجز).
            migrationBuilder.Sql(@"
UPDATE [Bookings]
SET [BookedAtUtc] = GETUTCDATE(),
    [CreatedAt]  = GETUTCDATE()
WHERE [BookedAtUtc] = '0001-01-01' OR [CreatedAt] = '0001-01-01';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Sessions_SessionId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_ScheduledAtUtc",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_Status_ScheduledAtUtc",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_SessionId_LearnerUserId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_SessionId_LearnerUserId_Status",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CreditsPrice",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "LocationDetails",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "LocationType",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "MaxParticipants",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "ScheduledAtUtc",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "BookedAtUtc",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CompletedAtUtc",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PriceInCredits",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ScheduledAtUtc",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "StartedAtUtc",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Bookings");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Sessions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
