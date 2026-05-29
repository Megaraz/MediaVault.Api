# Backend Engineer Workflow

Use this workflow when implementing or refactoring ASP.NET Core backend behavior in MediaVault.

## Goal

Make small, architecture-consistent backend changes that fit the current layering, preserve Result-based error handling, and improve the actual codebase without broad rewrites.

## Inputs

- Target backend task, bug, feature, or refactor.
- Optional scope: controller, service, validator, mapper, repository, auth flow, DTO, or persistence behavior.
- Optional constraint: no new packages, test-first, or stay within a specific layer.

## Workflow

1. Read `AGENTS.md` and `ACTIVE_CONTEXT.md` first.
2. Identify the owning layer and the narrowest real code path for the requested change.
3. Read the nearby implementation, DTOs, validators, repositories, and tests before editing.
4. Prefer the smallest backend change that fixes the real problem.
5. Preserve current backend conventions:
   - thin controllers
   - business logic in Application
   - Infrastructure-specific logic in Infrastructure
   - Result-based expected-outcome flow
   - centralized HTTP response mapping
6. Add or update focused tests when behavior changes.
7. Validate with the narrowest relevant backend test or build command.

## Repo-specific checks

- Keep `CancellationToken` flowing through async paths when the surrounding code already supports it.
- Do not bypass the current cookie-auth setup with ad hoc auth behavior.
- Match the existing type-specific media-entry patterns unless there is a strong reason not to.
- Keep new configuration aligned with the existing `IOptions<T>` + validation-on-start approach.
- Do not replace the current logging or Result flow casually when handling expected backend failures.

## Output format

1. Backend area affected
2. Planned or implemented change
3. Architecture and tradeoff notes
4. Validation result
5. Remaining risks or follow-up work
