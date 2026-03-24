using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace media_vault_app.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeRatingScaleToFiveStars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MediaEntry_Rating",
                table: "MediaEntries");

            migrationBuilder.Sql(@"
UPDATE MediaEntries
SET Rating = ROUND(Rating, 0) / 2.0
WHERE Rating > 5;");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MediaEntry_Rating",
                table: "MediaEntries",
                sql: "Rating >= 0 AND Rating <= 5 AND Rating * 2 = FLOOR(Rating * 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MediaEntry_Rating",
                table: "MediaEntries");

            migrationBuilder.Sql(@"
UPDATE MediaEntries
SET Rating = CASE
    WHEN Rating = 0 THEN 0.5
    ELSE Rating * 2
END;");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MediaEntry_Rating",
                table: "MediaEntries",
                sql: "Rating >= 0.5 AND Rating <= 10 AND Rating * 2 = FLOOR(Rating * 2)");
        }
    }
}