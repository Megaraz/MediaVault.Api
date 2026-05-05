using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace media_vault_app.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TvSeriesEntry_Season_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReleaseYear",
                table: "Seasons");

            migrationBuilder.RenameColumn(
                name: "TotalEpisodes",
                table: "MediaEntries",
                newName: "NumberOfSeasons");

            migrationBuilder.AddColumn<DateTime>(
                name: "AirDate",
                table: "Seasons",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdExternal",
                table: "Seasons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Seasons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Seasons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Overview",
                table: "Seasons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeasonNumber",
                table: "Seasons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AiringStatus",
                table: "MediaEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackdropImageUrl",
                table: "MediaEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAirDate",
                table: "MediaEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfEpisodes",
                table: "MediaEntries",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AirDate",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "IdExternal",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "Overview",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "SeasonNumber",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "AiringStatus",
                table: "MediaEntries");

            migrationBuilder.DropColumn(
                name: "BackdropImageUrl",
                table: "MediaEntries");

            migrationBuilder.DropColumn(
                name: "LastAirDate",
                table: "MediaEntries");

            migrationBuilder.DropColumn(
                name: "NumberOfEpisodes",
                table: "MediaEntries");

            migrationBuilder.RenameColumn(
                name: "NumberOfSeasons",
                table: "MediaEntries",
                newName: "TotalEpisodes");

            migrationBuilder.AddColumn<int>(
                name: "ReleaseYear",
                table: "Seasons",
                type: "int",
                nullable: true);
        }
    }
}
