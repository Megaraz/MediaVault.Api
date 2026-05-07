using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace media_vault_app.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GamePcRequirements_ValueObject_and_DateTime_to_DateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Seasons_MediaEntries_OwnerId",
                table: "Seasons");

            migrationBuilder.DropTable(
                name: "GamePcRequirements");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Seasons",
                newName: "TvSeriesEntryId");

            migrationBuilder.RenameIndex(
                name: "IX_Seasons_OwnerId",
                table: "Seasons",
                newName: "IX_Seasons_TvSeriesEntryId");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "AirDate",
                table: "Seasons",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "ReleaseDate",
                table: "MediaEntries",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "LastAirDate",
                table: "MediaEntries",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Genres",
                table: "MediaEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PcRequirements_Discriminator",
                table: "MediaEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PcRequirements_High",
                table: "MediaEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PcRequirements_Minimum",
                table: "MediaEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PcRequirements_Recommended",
                table: "MediaEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PcRequirements_Ultra",
                table: "MediaEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PcRequirements_VeryHigh",
                table: "MediaEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Seasons_MediaEntries_TvSeriesEntryId",
                table: "Seasons",
                column: "TvSeriesEntryId",
                principalTable: "MediaEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Seasons_MediaEntries_TvSeriesEntryId",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "PcRequirements_Discriminator",
                table: "MediaEntries");

            migrationBuilder.DropColumn(
                name: "PcRequirements_High",
                table: "MediaEntries");

            migrationBuilder.DropColumn(
                name: "PcRequirements_Minimum",
                table: "MediaEntries");

            migrationBuilder.DropColumn(
                name: "PcRequirements_Recommended",
                table: "MediaEntries");

            migrationBuilder.DropColumn(
                name: "PcRequirements_Ultra",
                table: "MediaEntries");

            migrationBuilder.DropColumn(
                name: "PcRequirements_VeryHigh",
                table: "MediaEntries");

            migrationBuilder.RenameColumn(
                name: "TvSeriesEntryId",
                table: "Seasons",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Seasons_TvSeriesEntryId",
                table: "Seasons",
                newName: "IX_Seasons_OwnerId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AirDate",
                table: "Seasons",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReleaseDate",
                table: "MediaEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastAirDate",
                table: "MediaEntries",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Genres",
                table: "MediaEntries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "GamePcRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    High = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Minimum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recommended = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ultra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VeryHigh = table.Column<string>(type: "nvarchar(max)", nullable: true)
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

            migrationBuilder.AddForeignKey(
                name: "FK_Seasons_MediaEntries_OwnerId",
                table: "Seasons",
                column: "OwnerId",
                principalTable: "MediaEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
