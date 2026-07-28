# Continuous integration

The `CI` GitHub Actions workflow validates the supported backend and web client
on every pull request and on pushes to `main`.

## Checks

- **Backend (.NET 10)** uses the repository's `net10.0` target and runs
  `dotnet test media-vault-app.slnx`.
- **Frontend (Node 24)** follows the checked-in Node 24 type/runtime line and
  runs `npm ci`, `npm run lint`, and `npm run build` from
  `media-vault-app.client`.

The workflow uses the checked-in npm lock file. It does not upload build
artifacts, test output, databases, or logs. Each job has a 15-minute timeout,
and a newer run for the same pull request or branch cancels an older run.

## Permissions and configuration

The workflow grants the GitHub token read-only repository-content access and
disables credential persistence after checkout. Pull-request code receives no
write permission, deployment credential, or repository secret.

No secret or provider credential is required. The test projects use
test-controlled dependencies and do not start the API, connect to external
metadata providers, or create the API's SQLite database.

## Run the same checks locally

From the repository root:

```powershell
dotnet test media-vault-app.slnx

Push-Location media-vault-app.client
npm ci
npm run lint
npm run build
Pop-Location
```

Generated .NET output, `node_modules`, `dist`, API SQLite files, and log
directories are ignored and must not be committed.

The README status badge is intentionally deferred until this workflow has a
stable successful result on the default branch.
