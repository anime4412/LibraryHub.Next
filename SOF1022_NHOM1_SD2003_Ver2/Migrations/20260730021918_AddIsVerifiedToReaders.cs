using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOF1022_NHOM1_SD2003_Ver2.Migrations
{
    /// <inheritdoc />
    public partial class AddIsVerifiedToReaders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Readers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Readers");
        }
    }
}
