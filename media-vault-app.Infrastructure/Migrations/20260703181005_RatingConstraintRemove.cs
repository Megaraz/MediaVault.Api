using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace media_vault_app.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RatingConstraintRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Season_Rating",
                table: "Seasons");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MediaEntry_Rating",
                table: "MediaEntries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Season_Rating",
                table: "Seasons",
                sql: "Rating >= 0 AND Rating <= 5 AND Rating * 2 = FLOOR(Rating * 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MediaEntry_Rating",
                table: "MediaEntries",
                sql: "Rating >= 0 AND Rating <= 5 AND Rating * 2 = FLOOR(Rating * 2)");
        }
    }
}
