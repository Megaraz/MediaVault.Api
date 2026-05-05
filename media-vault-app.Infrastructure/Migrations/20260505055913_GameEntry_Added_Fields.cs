using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace media_vault_app.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GameEntry_Added_Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DevStudioName",
                table: "MediaEntries",
                newName: "Website");

            migrationBuilder.AddColumn<int>(
                name: "MetacriticRating",
                table: "MediaEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Platforms",
                table: "MediaEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GamePcRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Minimum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recommended = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    High = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VeryHigh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ultra = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePcRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GamePcRequirements_MediaEntries_GameEntryId",
                        column: x => x.GameEntryId,
                        principalTable: "MediaEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GamePcRequirements_GameEntryId",
                table: "GamePcRequirements",
                column: "GameEntryId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GamePcRequirements");

            migrationBuilder.DropColumn(
                name: "MetacriticRating",
                table: "MediaEntries");

            migrationBuilder.DropColumn(
                name: "Platforms",
                table: "MediaEntries");

            migrationBuilder.RenameColumn(
                name: "Website",
                table: "MediaEntries",
                newName: "DevStudioName");
        }
    }
}
