# Backend dependency advisory baseline

The backend dependency audit runs on pull requests, pushes to `main`, a weekly
scheduled run, and manual dispatch through
`.github/workflows/dependency-audit.yml`. It restores
`media-vault-app.slnx`, asks the .NET CLI for the machine-readable vulnerable
package report, and compares each project/framework/package/version/severity
combination with `.github/dependency-advisory-baseline.json`.

The gate fails when a warning is not in the baseline. This includes a newly
introduced package, advisory, project, resolved version, severity, or package
transitivity. Baseline entries that are no longer reported produce a warning so
they can be removed after the remediation is confirmed.

## Current baseline

The only accepted warnings are the low-severity advisory
`GHSA-g4vj-cjjj-v7hg` for `NuGet.Packaging` and `NuGet.Protocol` 6.12.1 in the
API, Infrastructure, and test projects. They are transitive dependencies of
`Microsoft.VisualStudio.Web.CodeGeneration.Design` 10.0.2 through the
scaffolding packages. The current design-time package line does not provide a
compatible advisory-free dependency graph, so the baseline is retained instead
of forcing unrelated direct NuGet package pins.

`Megaraz` owns the baseline. It must be reviewed by 2026-11-15, or earlier when
`Microsoft.VisualStudio.Web.CodeGeneration.Design` or its scaffolding
dependencies publish a compatible advisory-free release. Update the JSON
baseline and this document together when an accepted warning is removed or its
reason changes.

The remediation also aligns the backend on EF Core 10.0.11, upgrades
`Microsoft.OpenApi` to 2.7.5 through ASP.NET Core OpenAPI 10.0.11, and resolves
SQLitePCLRaw 2.1.13, whose native SQLite library is 3.53.3. The frontend
dependency backlog remains out of scope.

## Local verification

Run from the repository root after restoring:

```powershell
dotnet restore media-vault-app.slnx
pwsh -NoProfile -File .github/scripts/check-dependency-advisories.ps1
```
