# Continuous integration and default-branch gates

The `CI` GitHub Actions workflow validates the backend on every pull request
and on pushes to `main`. Client CI lives in `Megaraz/MediaVault.Clients`.

## Checks

- **Backend (.NET 10)** uses the repository's `net10.0` target and runs
  `dotnet test media-vault-app.slnx`.
- **Dependency review** runs on pull requests and rejects newly introduced
  runtime vulnerabilities of moderate severity or higher. It keeps the
  official action's license inspection enabled without defining a separate
  repository allowlist or denylist.
- **CodeQL** uses GitHub's default setup and default query suite to analyze only
  C# and GitHub Actions code on pull requests, default-branch updates, and
  GitHub's scheduled cadence. JavaScript/TypeScript analysis belongs to
  `Megaraz/MediaVault.Clients` now that this repository is backend-only.

The workflow does not upload build artifacts, test output, databases, or logs.
Each job has a 15-minute timeout,
and a newer run for the same pull request or branch cancels an older run.

## Permissions and configuration

The checked-in workflows grant the GitHub token read-only repository-content
access and disable credential persistence after checkout. Pull-request code
receives no write permission, deployment credential, or repository secret.
Official GitHub actions are pinned to reviewed commit SHAs so a mutable tag
cannot silently change the code executed by CI.

No secret or provider credential is required. The test projects use
test-controlled dependencies and do not start the API, connect to external
metadata providers, or create the API's SQLite database.

## Protected `main` policy

The active repository ruleset named **Protect main quality and security gates**
targets only the default branch. It:

- requires changes to reach `main` through a pull request;
- requires the `Backend (.NET 10)` and `Dependency review` status checks
  against the latest `main` state;
- requires CodeQL code-scanning results and blocks error-level quality alerts or
  high-or-higher security alerts;
- blocks force pushes and deletion of `main`; and
- defines no routine bypass actor.

The required independent approval count is intentionally zero while MediaVault
has one maintainer. Requiring the only maintainer to obtain an independent
approval would make routine maintenance impossible. Raise the count to one as
soon as a trusted reviewer can reliably review changes; this is an explicit
solo-maintainer compromise, not independent review.

### Renaming a required check

Workflow and job names are repository policy because GitHub uses their exact
check contexts. Before intentionally renaming a required job:

1. keep the existing required context active;
2. introduce the new name on a pull request and wait for its successful check;
3. update the ruleset to require the new exact context while retaining the old
   one;
4. verify a fresh pull-request commit reports every required check and remains
   blocked while any one is pending; then
5. remove the obsolete context from the ruleset and workflow.

For CodeQL default setup, keep the configured languages limited to `actions`
and `csharp`, and keep the ruleset's required tool name `CodeQL` aligned with
the tool reported by GitHub code scanning. Do not add an advanced
`.github/workflows/codeql.yml` while default setup is enabled: GitHub rejects
advanced CodeQL uploads when default setup owns analysis. Re-verify the
ruleset after any intentional switch between default and advanced setup.

### Emergency recovery

There is no standing administrator or maintainer bypass. If a GitHub outage or
misconfigured required check blocks an urgent security or repository-recovery
change, the repository owner may temporarily edit the ruleset in **Settings >
Rules > Rulesets**. Record the reason, affected commit or pull request, time, and
exact temporary change in the relevant private operational record; use the
narrowest change; restore the ruleset immediately; and re-run the API checks
below. Do not use this path for routine merges or to ignore a failing check.

After any ruleset or workflow change, verify the live policy:

```powershell
gh api repos/Megaraz/MediaVault.Api/code-scanning/default-setup
gh api repos/Megaraz/MediaVault.Api/rulesets
gh api repos/Megaraz/MediaVault.Api/rules/branches/main
```

## Run the same checks locally

From the repository root:

```powershell
dotnet test media-vault-app.slnx
```

Generated .NET output, API SQLite files, and log directories are ignored and
must not be committed.
