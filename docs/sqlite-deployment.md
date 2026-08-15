# SQLite deployment runbook

This is the supported first-release deployment shape for the API:

- Run exactly **one API instance**.
- Store one SQLite database on an absolute path inside a persistent volume.
- Run EF Core migrations as a separate deployment step before serving traffic.
- Use `/health/ready` as the rollout/readiness gate.
- Schedule backups outside the API process and perform a restore drill against a
  copied database.

SQLite remains the only supported database for this deployment shape. PostgreSQL,
multi-instance SQLite, distributed locks, and automatic migration-on-startup are
not part of this issue.

## Configure the persistent database

The API requires `ConnectionStrings:Default` in production. Set
`ASPNETCORE_ENVIRONMENT=Production` and provide an absolute SQLite file path
through deployment configuration:

```text
ConnectionStrings__Default=Data Source=/var/lib/mediavault/mediavault.db
```

On Windows, use an absolute path on the persistent volume instead, for example:

```text
ConnectionStrings__Default=Data Source=C:\MediaVault\data\mediavault.db
```

The volume directory must exist and be writable by the API process. Production
startup rejects relative paths, `:memory:`, and other non-persistent SQLite
connection strings. The checked-in
[`appsettings.Production.example.json`](../media-vault-app.API/appsettings.Production.example.json)
shows the Linux/container form; it is an example, not a secret-bearing runtime
file.

## Deploy and gate traffic

Stop the existing instance or otherwise ensure that no API instance is serving
the database while the schema changes are being applied. Run the migration
command with the same production connection settings:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ConnectionStrings__Default = "Data Source=C:\MediaVault\data\mediavault.db"

dotnet ef database update `
  --project media-vault-app.Infrastructure `
  --startup-project media-vault-app.API `
  --configuration Release
```

The user-identifier migration trims and lowercases existing usernames and email
addresses before restoring case-insensitive unique indexes. If legacy rows collide
after that canonicalization, the migration aborts without merging or deleting
accounts; resolve the duplicate data deliberately and rerun the migration.

Do not call `Database.Migrate()` from application startup. The explicit command
keeps schema changes observable and allows the deployment operator to stop when
a migration fails. Start one API instance only after the command succeeds, then
probe:

```powershell
Invoke-RestMethod https://api.example.test/health/ready
```

The anonymous readiness response has this shape:

```json
{
  "status": "Healthy",
  "checks": {
    "database": {
      "status": "Healthy",
      "data": {
        "database": "sqlite",
        "connectivity": "ok",
        "migrationState": "current",
        "pendingMigrationCount": 0
      }
    }
  }
}
```

The endpoint returns `503 Unhealthy` when SQLite cannot be opened or pending
migrations remain. Do not route user traffic to the instance until it returns
`200 Healthy`.

## Back up the database

Run a periodic host-managed backup using SQLite's online backup command. This
works while the single API instance is running and avoids copying an
in-progress WAL transaction:

```powershell
$databasePath = "C:\MediaVault\data\mediavault.db"
$backupPath = "C:\MediaVault\backups\mediavault-2026-08-15.db"

sqlite3 $databasePath ".backup '$backupPath'"
sqlite3 $backupPath "PRAGMA integrity_check;"
```

Require `ok` from `PRAGMA integrity_check;`, restrict backup-file permissions,
and apply the host's approved retention and encryption policy. Do not copy only
the main `.db` file while the API is writing and do not commit backups to this
repository. The backup schedule and retention policy belong to the deployment
host; the API does not run a backup job.

## Restore drill

Perform this drill at least once before relying on a deployment and repeat it
after material hosting or migration changes:

1. Create disposable source, backup, and restored-volume directories outside the
   repository, each with an absolute path.
2. Point `ConnectionStrings__Default` at the disposable source database, run
   the migration command above, and start exactly one API instance.
3. Exercise one representative authenticated read/write request, then stop the
   instance cleanly.
4. Run SQLite's `.backup` command to create one backup copy and verify it with
   `PRAGMA integrity_check;`.
5. Restore the backup into a fresh restored-volume location. Point the API at
   that absolute path and run the migration command against the restored copy.
6. Start exactly one API instance against the restored database. Confirm
   `/health/ready` returns `200 Healthy` with `migrationState: "current"`.
7. Repeat the representative authenticated read/write request and record the
   result, then remove only the disposable drill directories.

Record the date, source and restored volume identifiers, migration result,
integrity-check result, readiness response, and representative request result.
The supported topology remains one API instance even when the restore drill is
complete.
