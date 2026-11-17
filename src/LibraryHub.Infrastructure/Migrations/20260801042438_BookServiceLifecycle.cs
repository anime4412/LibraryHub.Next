using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BookServiceLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorrelationKey",
                table: "Notifications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastLifecycleOperationKey",
                table: "Books",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RecallDeadline",
                table: "Books",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RecallRequestedAt",
                table: "Books",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RestoreReason",
                table: "Books",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RestoredAt",
                table: "Books",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RestoredByAccountId",
                table: "Books",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SuspendedAt",
                table: "Books",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SuspendedByAccountId",
                table: "Books",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspensionReason",
                table: "Books",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SuspensionType",
                table: "Books",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [Books]
                SET [Status] = 5,
                    [SuspensionType] = 0,
                    [SuspensionReason] = N'Chuyển đổi an toàn từ bản ghi xóa mềm trước khi áp dụng vòng đời hai giai đoạn.',
                    [SuspendedAt] = COALESCE([DeletedAt], SYSUTCDATETIME()),
                    [IsDeleted] = 0,
                    [DeletedAt] = NULL
                WHERE [IsDeleted] = 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientAccountId_Type_CorrelationKey",
                table: "Notifications",
                columns: new[] { "RecipientAccountId", "Type", "CorrelationKey" },
                unique: true,
                filter: "[CorrelationKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE [Books]
                SET [IsDeleted] = 1,
                    [DeletedAt] = [SuspendedAt]
                WHERE [Status] = 5
                  AND [SuspensionReason] = N'Chuyển đổi an toàn từ bản ghi xóa mềm trước khi áp dụng vòng đời hai giai đoạn.';
                """);

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RecipientAccountId_Type_CorrelationKey",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CorrelationKey",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "LastLifecycleOperationKey",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "RecallDeadline",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "RecallRequestedAt",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "RestoreReason",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "RestoredAt",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "RestoredByAccountId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "SuspendedAt",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "SuspendedByAccountId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "SuspensionReason",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "SuspensionType",
                table: "Books");
        }
    }
}
