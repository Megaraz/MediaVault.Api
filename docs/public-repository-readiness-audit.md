# Public repository readiness audit

This record covers issue #56. It is intentionally limited to the API/web repository; the Android repository has its own readiness work.

## Scope and method

The audit was run on 2026-07-28 against every reachable local and remote ref:

- `main`, `result-migration-p1`, and `codex/issue-56-public-readiness-audit`;
- their `origin/*` tracking refs; and
- no tags.

Tracked files, historical path names, current configuration keys, machine-local paths, remote heads/tags, and GitHub Actions metadata were reviewed. No values from the database, local user secrets, or Actions logs are recorded here.

Gitleaks 8.30.1 completed successfully with no findings:

```powershell
gitleaks git . --log-opts="--all" --redact=100 --report-format json
```

The command scanned all reachable history and used full redaction. Its JSON report had zero findings. This is evidence against detected plaintext credentials; it does not make the tracked runtime database safe to publish.

## Findings

### Runtime database history is development-only

The following files are tracked in the current tree and occur in the reachable history at commits `1fe6450` and `37c1ba6`:

- `media-vault-app.API/mediavault.db`
- `media-vault-app.API/mediavault.db-wal`
- `media-vault-app.API/mediavault.db-shm`

The database contains a `Users` table with username, email, and password-hash fields, along with user-owned media and review data. The audit did not read or reproduce any values. The repository owner confirmed that every row is deliberately disposable local development/test data: the account is not used elsewhere, no password is reused, and the media/reviews are junk data.

No plaintext credential was detected by Gitleaks. The database schema itself is already represented by the checked-in entities and EF Core migrations, so its historical presence does not add a material public-exposure risk after the owner's assessment. The files are nevertheless removed from the current tree and ignored going forward because runtime state does not belong in source control.

### Configuration and machine-local data

The checked-in `appsettings.json` and `appsettings.Development.json` contain logging settings only. Runtime configuration is bound from `ConnectionStrings`, `Jwt`, and `ExternalApis`; the safe placeholders in `media-vault-app.API/appsettings.example.json` document the required shape without containing usable credentials.

The API project already has a user-secrets ID. Keep local credentials out of the repository with user secrets or environment variables. For example:

```powershell
dotnet user-secrets --project media-vault-app.API set "ConnectionStrings:Default" "Data Source=mediavault.db"
dotnet user-secrets --project media-vault-app.API set "Jwt:SecretKey" "<local random secret of at least 32 characters>"
dotnet user-secrets --project media-vault-app.API set "Jwt:Issuer" "MediaVault.Local"
dotnet user-secrets --project media-vault-app.API set "Jwt:Audience" "MediaVault.Local"
dotnet user-secrets --project media-vault-app.API set "ExternalApis:Rawg:BaseUrl" "https://api.rawg.io/api/"
dotnet user-secrets --project media-vault-app.API set "ExternalApis:Rawg:ApiKey" "<rawg key>"
dotnet user-secrets --project media-vault-app.API set "ExternalApis:Tmdb:BaseUrl" "https://api.themoviedb.org/3/"
dotnet user-secrets --project media-vault-app.API set "ExternalApis:Tmdb:ApiAccessToken" "<tmdb token>"
dotnet user-secrets --project media-vault-app.API set "ExternalApis:GoogleBooks:BaseUrl" "https://www.googleapis.com/books/v1/"
dotnet user-secrets --project media-vault-app.API set "ExternalApis:GoogleBooks:ApiKey" "<google books key>"
```

The `.gitignore` rules added by this issue protect conventional `.env` files, certificate/key exports, and this API's SQLite database/WAL/SHM files. They do not delete existing local runtime state.

### GitHub Actions exposure

There are no checked-in GitHub Actions workflow files, no Actions secrets or variables, and no retained workflow artifacts. The repository has three historical Copilot-generated workflow runs (2026-03-22 through 2026-03-24). Their logs are no longer retrievable through the GitHub CLI/API, so their contents could not be re-scanned. Record this limitation before changing visibility; there is no retained artifact to expose from those runs.

## History-rewrite decision

A normal commit or pull request cannot remove the database blobs from ancestor commits. A coordinated history rewrite is therefore the only way to remove them from every reachable historical ref.

It is not required for the planned public release because the owner confirmed that the historical data is entirely synthetic and disposable, and the audit found no live credentials. Avoiding a rewrite preserves current commit identities and avoids unnecessary force-push coordination.

Revisit this decision and perform a coordinated rewrite if later evidence shows that a real credential, reusable password, personal data, or non-disposable review was present.

The owner may still choose a rewrite for repository hygiene, but it is a discretionary cleanup rather than a security prerequisite.

## Clean-clone verification

Run the following from a new clone. These commands use only checked-in projects, the lock file, and public package registries.

```powershell
dotnet restore media-vault-app.slnx
dotnet test media-vault-app.slnx

Push-Location media-vault-app.client
npm ci
npm run lint
npm run build
Pop-Location
```

Then confirm the repository contains no tracked runtime database, local environment, or key files:

```powershell
git ls-files | Select-String -Pattern '\.(db|db-wal|db-shm|pfx|pem|key)$|(^|/)\.env($|\.)'
git diff --check
```

The implementation in this issue does not change API routes, authentication behavior, JSON contracts, persistence schema, web behavior, or Android behavior.
