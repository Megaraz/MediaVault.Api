using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace media_vault_app.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IdExternal = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Rating = table.Column<decimal>(type: "TEXT", precision: 3, scale: 1, nullable: false),
                    Review = table.Column<string>(type: "TEXT", nullable: true),
                    Genres = table.Column<string>(type: "TEXT", nullable: false),
                    Overview = table.Column<string>(type: "TEXT", nullable: true),
                    ReleaseDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    MediaType = table.Column<int>(type: "INTEGER", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BookEntry_Author = table.Column<string>(type: "TEXT", nullable: true),
                    MetacriticRating = table.Column<int>(type: "INTEGER", nullable: true),
                    Website = table.Column<string>(type: "TEXT", nullable: true),
                    Platforms = table.Column<string>(type: "TEXT", nullable: true),
                    HoursPlayed = table.Column<int>(type: "INTEGER", nullable: true),
                    PcRequirements_Discriminator = table.Column<string>(type: "TEXT", nullable: true),
                    PcRequirements_High = table.Column<string>(type: "TEXT", nullable: true),
                    PcRequirements_Minimum = table.Column<string>(type: "TEXT", nullable: true),
                    PcRequirements_Recommended = table.Column<string>(type: "TEXT", nullable: true),
                    PcRequirements_Ultra = table.Column<string>(type: "TEXT", nullable: true),
                    PcRequirements_VeryHigh = table.Column<string>(type: "TEXT", nullable: true),
                    Author = table.Column<string>(type: "TEXT", nullable: true),
                    RuntimeMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    BackdropImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    LastAirDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    NumberOfSeasons = table.Column<int>(type: "INTEGER", nullable: true),
                    NumberOfEpisodes = table.Column<int>(type: "INTEGER", nullable: true),
                    AiringStatus = table.Column<string>(type: "TEXT", nullable: true),
                    TotalWatchedEpisodes = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaEntries", x => x.Id);
                    table.CheckConstraint("CK_MediaEntry_Rating", "Rating >= 0 AND Rating <= 5 AND Rating * 2 = FLOOR(Rating * 2)");
                    table.ForeignKey(
                        name: "FK_MediaEntries_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TvSeriesEntryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IdExternal = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Overview = table.Column<string>(type: "TEXT", nullable: true),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    SeasonNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    AirDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    WatchedEpisodes = table.Column<int>(type: "INTEGER", nullable: false),
                    Episodes = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Rating = table.Column<decimal>(type: "TEXT", precision: 3, scale: 1, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                    table.CheckConstraint("CK_Season_Rating", "Rating >= 0 AND Rating <= 5 AND Rating * 2 = FLOOR(Rating * 2)");
                    table.ForeignKey(
                        name: "FK_Seasons_MediaEntries_TvSeriesEntryId",
                        column: x => x.TvSeriesEntryId,
                        principalTable: "MediaEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaEntries_OwnerId",
                table: "MediaEntries",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_TvSeriesEntryId",
                table: "Seasons",
                column: "TvSeriesEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropTable(
                name: "MediaEntries");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
