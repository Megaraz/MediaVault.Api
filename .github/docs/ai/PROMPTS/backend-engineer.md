# Backend Engineer Workflow

Use this workflow when implementing or refactoring ASP.NET Core backend behavior in MediaVault.

## Goal

Make small, architecture-consistent backend changes that fit the current layering, preserve Result-based error handling, and improve the actual codebase without broad rewrites.

## Inputs

- Target backend task, bug, feature, or refactor.
- Optional scope: controller, service, validator, mapper, repository, auth flow, DTO, or persistence behavior.
- Optional constraint: no new packages, test-first, or stay within a specific layer.

## Required context

- Read `AGENTS.md` first.
- Read `ACTIVE_CONTEXT.md` if it exists.
- Inspect the relevant code, tests, configuration, and docs before making claims or edits.
- Treat the current codebase as more authoritative than this workflow file if they disagree.
- Separate current implementation from desired future state.
- If `ACTIVE_CONTEXT.md` does not exist, continue without failing.

## Workflow

1. Identify the owning layer and the narrowest real code path for the requested change.
2. Read the nearby implementation, DTOs, validators, repositories, and tests before editing.
3. Prefer the smallest backend change that fixes the real problem.
4. Preserve current backend conventions:
   - thin controllers
   - business logic in Application
   - Infrastructure-specific logic in Infrastructure
   - Result-based expected-outcome flow
   - centralized HTTP response mapping
5. Add or update focused tests when behavior changes.
6. Validate with the narrowest relevant backend test or build command.

## Repo-specific checks

- Controllers should stay thin.
- Business workflows belong in Application.
- EF Core and persistence concerns belong in Infrastructure.
- Expected failures should flow through `Result` or `Result<T>`.
- HTTP status-code decisions should stay centralized in the existing result-mapping layer.
- Do not introduce FluentValidation casually because the repo currently uses custom validators.
- Keep `CancellationToken` flowing through async paths when the surrounding code already supports it.
- Do not bypass the current cookie-auth setup with ad hoc auth behavior.
- Do not replace cookie auth with JWT without an explicit architectural decision.
- Match the existing type-specific media-entry patterns unless there is a strong reason not to.
- Keep new configuration aligned with the existing `IOptions<T>` + validation-on-start approach.
- Do not replace the current logging or Result flow casually when handling expected backend failures.
- Do not invent a parallel error-logging approach.
- Do not create new top-level folders, projects, architectural layers, libraries, or naming schemes without first checking whether an existing convention already fits.
- Do not delete files, remove public APIs, rename projects, or perform broad formatting-only changes unless explicitly asked or clearly required by the current task.

## Output format

1. Backend area affected
2. Current behavior observed
3. Planned or implemented change
4. Architecture and tradeoff notes
5. Tests added or updated
6. Validation result
7. Remaining risks or follow-up work

## Validation fallback

If validation commands cannot be run, state exactly:

- what changed
- what was not validated
- which command should be run manually
