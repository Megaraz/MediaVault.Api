using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace media_vault_app.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Author_Relation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaEntries_Authors_AuthorId",
                table: "MediaEntries");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropIndex(
                name: "IX_MediaEntries_AuthorId",
                table: "MediaEntries");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "MediaEntries");

            migrationBuilder.AddColumn<bool>(
                name: "Watched",
                table: "Seasons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "MediaEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BookEntry_Author",
                table: "MediaEntries",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Watched",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "Author",
                table: "MediaEntries");

            migrationBuilder.DropColumn(
                name: "BookEntry_Author",
                table: "MediaEntries");

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "MediaEntries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeCountry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    YearOfBirth = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaEntries_AuthorId",
                table: "MediaEntries",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaEntries_Authors_AuthorId",
                table: "MediaEntries",
                column: "AuthorId",
                principalTable: "Authors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
