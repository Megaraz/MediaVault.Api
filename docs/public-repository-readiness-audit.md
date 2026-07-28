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

### Runtime database history is sensitive

The following files are tracked in the current tree and occur in the reachable history at commits `1fe6450` and `37c1ba6`:

- `media-vault-app.API/mediavault.db`
- `media-vault-app.API/mediavault.db-wal`
- `media-vault-app.API/mediavault.db-shm`

The database contains a `Users` table with username, email, and password-hash fields, along with user-owned media and review data. The audit did not read or reproduce any values. These are private runtime data, so the files must not be published and their reachable history must be removed before the repository becomes public.

No plaintext credential was detected by Gitleaks. As a defence-in-depth measure, the owner should reset the password of any account represented by this database before the rewritten repository is published, because a password hash was present in history.

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

## Required history-rewrite gate

A normal commit or pull request cannot remove the database blobs from ancestor commits. Do not change repository visibility until a coordinated rewrite has removed them from every public-bound ref.

Before that rewrite:

1. Confirm the database is preserved only in an approved private backup and reset any password associated with the recorded account.
2. Freeze merges and new branches, notify all clone owners, and identify every branch, tag, pull-request head, and backup that can reintroduce the old commits.
3. In a fresh private clone, use a history-rewrite tool to remove exactly the three database paths from all refs. Re-run the Gitleaks command above and the tracked-file checks against the rewritten clone.
4. Have the repository owner approve the new history, coordinate protected-branch handling, force-push each rewritten public-bound ref, and ask collaborators to reclone or reset according to the agreed plan.
5. Verify that GitHub no longer exposes the old blobs or relevant Actions artifacts before changing repository visibility.

This gate is deliberately not executed by the issue pull request: it changes published commit identities and requires explicit owner coordination.

## Clean-clone verification

Run the following from a new clone after the history-rewrite gate is complete. These commands use only checked-in projects, the lock file, and public package registries.

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
