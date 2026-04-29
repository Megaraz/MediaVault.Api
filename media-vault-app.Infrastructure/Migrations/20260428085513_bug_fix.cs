using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace media_vault_app.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class bug_fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Watched",
                table: "Seasons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Watched",
                table: "Seasons",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
