using Microsoft.Data.Sqlite;

namespace media_vault_app.API.Configuration;

internal static class SqliteConnectionStringPolicy
{
    public static void ValidateForProduction(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'Default' must be configured in Production.");
        }

        SqliteConnectionStringBuilder sqliteConnectionString;
        try
        {
            sqliteConnectionString = new SqliteConnectionStringBuilder(connectionString);
        }
        catch (ArgumentException exception)
        {
            throw new InvalidOperationException(
                "Connection string 'Default' must be a valid SQLite connection string in Production.",
                exception);
        }

        var dataSource = sqliteConnectionString.DataSource;
        if (sqliteConnectionString.Mode == SqliteOpenMode.Memory ||
            string.IsNullOrWhiteSpace(dataSource) ||
            string.Equals(dataSource, ":memory:", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Connection string 'Default' must use a persistent SQLite database file in Production.");
        }

        if (!Path.IsPathFullyQualified(dataSource))
        {
            throw new InvalidOperationException(
                "Connection string 'Default' must use an absolute SQLite database path in Production.");
        }
    }
}
