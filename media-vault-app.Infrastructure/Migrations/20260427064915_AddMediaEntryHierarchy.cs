using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace media_vault_app.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaEntryHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaEntries_Users_OwnerId",
                table: "MediaEntries");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "MediaEntries");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "MediaEntries",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Genre",
                table: "MediaEntries",
                newName: "Genres");

            migrationBuilder.RenameIndex(
                name: "IX_MediaEntries_OwnerId",
                table: "MediaEntries",
                newName: "IX_MediaEntries_UserId");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "MediaEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "MediaEntries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DevStudioName",
                table: "MediaEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HoursPlayed",
                table: "MediaEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalEpisodes",
                table: "MediaEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalWatchedEpisodes",
                table: "MediaEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "MediaEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HomeCountry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YearOfBirth = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TvSeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReleaseYear = table.Column<int>(type: "int", nullable: true),
                    WatchedEpisodes = table.Column<int>(type: "int", nullable: false),
                    Episodes = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(3,1)", precision: 3, scale: 1, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                    table.CheckConstraint("CK_Season_Rating", "Rating >= 0 AND Rating <= 5 AND Rating * 2 = FLOOR(Rating * 2)");
                    table.ForeignKey(
                        name: "FK_Seasons_MediaEntries_TvSeriesId",
                        column: x => x.TvSeriesId,
                        principalTable: "MediaEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaEntries_AuthorId",
                table: "MediaEntries",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_TvSeriesId",
                table: "Seasons",
                column: "TvSeriesId");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaEntries_Authors_AuthorId",
                table: "MediaEntries",
                column: "AuthorId",
                principalTable: "Authors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MediaEntries_Users_UserId",
                table: "MediaEntries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaEntries_Authors_AuthorId",
                table: "MediaEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaEntries_Users_UserId",
                table: "MediaEntries");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropIndex(
                name: "IX_MediaEntries_AuthorId",
                table: "MediaEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "MediaEntries");

            migrationBuilder.DropColumn(
                name: "DevStudioName",
                table: "MediaEntries");

            migrationBuilder.DropColumn(
                name: "HoursPlayed",
                table: "MediaEntries");

            migrationBuilder.DropColumn(
                name: "TotalEpisodes",
                table: "MediaEntries");

            migrationBuilder.DropColumn(
                name: "TotalWatchedEpisodes",
                table: "MediaEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "MediaEntries");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "MediaEntries",
                newName: "OwnerId");

            migrationBuilder.RenameColumn(
                name: "Genres",
                table: "MediaEntries",
                newName: "Genre");

            migrationBuilder.RenameIndex(
                name: "IX_MediaEntries_UserId",
                table: "MediaEntries",
                newName: "IX_MediaEntries_OwnerId");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "MediaEntries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "MediaEntries",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaEntries_Users_OwnerId",
                table: "MediaEntries",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
