# SharedKernel Maintainer Workflow

Use this workflow for any change that touches `Rasmus.SharedKernel` or its public tests.

## Goal

Keep SharedKernel reusable, stable, well-tested, and free from MediaVault-specific assumptions.

## Inputs

- Target type, namespace, behavior, or proposed new abstraction.
- Optional reason: missing tests, API cleanup, documentation, or reusable feature request.

## Required context

- Read `AGENTS.md` first.
- Read `ACTIVE_CONTEXT.md` if it exists.
- Inspect the relevant code, tests, configuration, and docs before making claims or edits.
- Treat the current codebase as more authoritative than this workflow file if they disagree.
- Separate current implementation from desired future state.
- If `ACTIVE_CONTEXT.md` does not exist, continue without failing.

## Workflow

1. Confirm that the requested change truly belongs in SharedKernel rather than MediaVault.
2. Evaluate the public API for clarity, naming, coupling, and long-term maintainability.
3. Prefer the simplest reusable shape that still solves the real problem.
4. Compare the proposed change against a simpler alternative or keeping the logic app-local.
5. Add or update tests in `Rasmus.SharedKernel.Tests` for public behavior.
6. Update documentation when public behavior or expectations change.

## Repo-specific checks

- SharedKernel currently contains the Result Pattern, validator-related abstractions, repository interfaces, entity interfaces, and error logging contracts.
- SharedKernel changes must make sense outside MediaVault.
- Reject app-specific convenience APIs.
- Avoid names, error messages, or assumptions that only make sense in MediaVault.
- Public API changes require tests.
- Public behavior changes require documentation.
- Preserve current Result semantics unless the change is deliberate, justified, and fully covered by tests.
- Avoid new dependencies unless they are strongly justified.
- Favor boring library code over clever abstractions.
- Do not create new top-level folders, projects, architectural layers, libraries, or naming schemes without first checking whether an existing convention already fits.
- Do not delete files, remove public APIs, rename projects, or perform broad formatting-only changes unless explicitly asked or clearly required by the current task.

## SharedKernel API design checklist

Before changing SharedKernel, ask:

- Would this still make sense in another .NET project?
- Is the name understandable without MediaVault context?
- Is the behavior easy to test in isolation?
- Are failure modes clear?
- Does the API avoid surprising nullability?
- Does the API avoid hidden side effects?
- Is this better as reusable library code, or should it stay app-local?

## Output format

1. Does this belong in SharedKernel?
2. Current public behavior
3. Recommended change
4. Simpler app-local alternative considered
5. API compatibility impact
6. Test impact
7. Documentation impact
8. Remaining risks

## Validation fallback

If validation commands cannot be run, state exactly:

- what changed
- what was not validated
- which command should be run manually
