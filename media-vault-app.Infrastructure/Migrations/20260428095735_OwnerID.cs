using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace media_vault_app.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OwnerID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaEntries_Users_UserId",
                table: "MediaEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Seasons_MediaEntries_TvSeriesId",
                table: "Seasons");

            migrationBuilder.RenameColumn(
                name: "TvSeriesId",
                table: "Seasons",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Seasons_TvSeriesId",
                table: "Seasons",
                newName: "IX_Seasons_OwnerId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "MediaEntries",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_MediaEntries_UserId",
                table: "MediaEntries",
                newName: "IX_MediaEntries_OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaEntries_Users_OwnerId",
                table: "MediaEntries",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Seasons_MediaEntries_OwnerId",
                table: "Seasons",
                column: "OwnerId",
                principalTable: "MediaEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaEntries_Users_OwnerId",
                table: "MediaEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Seasons_MediaEntries_OwnerId",
                table: "Seasons");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Seasons",
                newName: "TvSeriesId");

            migrationBuilder.RenameIndex(
                name: "IX_Seasons_OwnerId",
                table: "Seasons",
                newName: "IX_Seasons_TvSeriesId");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "MediaEntries",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_MediaEntries_OwnerId",
                table: "MediaEntries",
                newName: "IX_MediaEntries_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaEntries_Users_UserId",
                table: "MediaEntries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Seasons_MediaEntries_TvSeriesId",
                table: "Seasons",
                column: "TvSeriesId",
                principalTable: "MediaEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
