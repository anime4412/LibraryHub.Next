using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOF1022_NHOM1_SD2003_Ver2.Migrations
{
    /// <inheritdoc />
    public partial class RenameSupportTicketAssignedEmployeeToAssignedStaff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Employees_AssignedEmployeeId",
                table: "SupportTickets");

            migrationBuilder.RenameColumn(
                name: "AssignedEmployeeId",
                table: "SupportTickets",
                newName: "AssignedStaffId");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTickets_AssignedEmployeeId",
                table: "SupportTickets",
                newName: "IX_SupportTickets_AssignedStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Readers_AssignedStaffId",
                table: "SupportTickets",
                column: "AssignedStaffId",
                principalTable: "Readers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Readers_AssignedStaffId",
                table: "SupportTickets");

            migrationBuilder.RenameColumn(
                name: "AssignedStaffId",
                table: "SupportTickets",
                newName: "AssignedEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTickets_AssignedStaffId",
                table: "SupportTickets",
                newName: "IX_SupportTickets_AssignedEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Employees_AssignedEmployeeId",
                table: "SupportTickets",
                column: "AssignedEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
