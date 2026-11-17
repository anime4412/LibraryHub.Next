using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnReplaySnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FineAmount",
                table: "BorrowReturns",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "FineId",
                table: "BorrowReturns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResultSlipId",
                table: "BorrowReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ResultStatus",
                table: "BorrowReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE [br]
                SET [ResultSlipId] = [bd].[BorrowSlipId],
                    [ResultStatus] = [bs].[Status],
                    [FineId] = [f].[Id],
                    [FineAmount] = COALESCE([f].[Amount], 0)
                FROM [BorrowReturns] AS [br]
                INNER JOIN [BorrowDetails] AS [bd] ON [bd].[Id] = [br].[BorrowDetailId]
                INNER JOIN [BorrowSlips] AS [bs] ON [bs].[Id] = [bd].[BorrowSlipId]
                OUTER APPLY
                (
                    SELECT TOP (1) [candidate].[Id], [candidate].[Amount]
                    FROM [Fines] AS [candidate]
                    WHERE [candidate].[BorrowSlipId] = [bd].[BorrowSlipId]
                    ORDER BY [candidate].[CreatedAt] DESC, [candidate].[Id] DESC
                ) AS [f];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FineAmount",
                table: "BorrowReturns");

            migrationBuilder.DropColumn(
                name: "FineId",
                table: "BorrowReturns");

            migrationBuilder.DropColumn(
                name: "ResultSlipId",
                table: "BorrowReturns");

            migrationBuilder.DropColumn(
                name: "ResultStatus",
                table: "BorrowReturns");
        }
    }
}
