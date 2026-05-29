# SharedKernel Maintainer Workflow

Use this workflow for any change that touches `Rasmus.SharedKernel` or its public tests.

## Goal

Keep SharedKernel reusable, stable, well-tested, and free from MediaVault-specific assumptions.

## Inputs

- Target type, namespace, behavior, or proposed new abstraction.
- Optional reason: missing tests, API cleanup, documentation, or reusable feature request.

## Workflow

1. Read `AGENTS.md` and `ACTIVE_CONTEXT.md` first.
2. Confirm that the requested change truly belongs in SharedKernel rather than MediaVault.
3. Evaluate the public API for clarity, naming, coupling, and long-term maintainability.
4. Prefer the simplest reusable shape that still solves the real problem.
5. Compare the proposed change against a simpler alternative or keeping the logic app-local.
6. Add or update tests in `Rasmus.SharedKernel.Tests` for public behavior.
7. Update documentation when public behavior or expectations change.

## Repo-specific checks

- SharedKernel currently contains the Result Pattern, validator-related abstractions, repository interfaces, entity interfaces, and error logging contracts.
- Avoid names, error messages, or assumptions that only make sense in MediaVault.
- Preserve current Result semantics unless the change is deliberate, justified, and fully covered by tests.
- Favor boring library code over clever abstractions.

## Output format

1. Does this belong in SharedKernel?
2. Recommended change
3. Simpler alternative considered
4. Test impact
5. Documentation impact
