using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOF1022_NHOM1_SD2003_Ver2.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileTagPins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfileTagPins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfileOwnerId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    TagKeysJson = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PresetMessageKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileTagPins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileTagPins_Readers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Readers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProfileTagPins_Readers_ProfileOwnerId",
                        column: x => x.ProfileOwnerId,
                        principalTable: "Readers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileTagPins_AuthorId",
                table: "ProfileTagPins",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileTagPins_ProfileOwnerId",
                table: "ProfileTagPins",
                column: "ProfileOwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileTagPins");
        }
    }
}
