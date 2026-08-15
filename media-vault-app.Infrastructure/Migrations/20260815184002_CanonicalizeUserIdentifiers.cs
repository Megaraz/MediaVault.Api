using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace media_vault_app.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CanonicalizeUserIdentifiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            // Abort rather than merge or delete legacy accounts that collide after canonicalization.
            migrationBuilder.Sql(
                """
                CREATE TEMP TRIGGER "__MediaVaultCanonicalUserConflictGuard"
                BEFORE UPDATE OF "Username", "Email" ON "Users"
                WHEN EXISTS (
                    SELECT 1
                    FROM "Users" AS "ExistingUser"
                    WHERE "ExistingUser"."Id" <> OLD."Id"
                      AND (
                           lower(trim("ExistingUser"."Username")) = lower(trim(NEW."Username"))
                        OR lower(trim("ExistingUser"."Email")) = lower(trim(NEW."Email"))
                      )
                )
                BEGIN
                    SELECT RAISE(ABORT, 'Users contain identifiers that collide after canonicalization.');
                END;
                """);

            migrationBuilder.Sql(
                """
                UPDATE "Users"
                SET "Username" = lower(trim("Username")),
                    "Email" = lower(trim("Email"));
                """);

            migrationBuilder.Sql(
                """DROP TRIGGER "__MediaVaultCanonicalUserConflictGuard";""");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "TEXT",
                nullable: false,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "TEXT",
                nullable: false,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldCollation: "NOCASE");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldCollation: "NOCASE");
        }
    }
}
