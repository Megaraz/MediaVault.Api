using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace media_vault_app.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReleaseYear_to_ReleaseDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReleaseYear",
                table: "MediaEntries");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReleaseDate",
                table: "MediaEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReleaseDate",
                table: "MediaEntries");

            migrationBuilder.AddColumn<int>(
                name: "ReleaseYear",
                table: "MediaEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
