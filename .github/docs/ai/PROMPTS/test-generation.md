# Test Generation Workflow

Use this workflow when generating or updating tests for backend or SharedKernel code.

## Goal

Add focused tests that match existing patterns, prove behavior, and keep the scope as small as possible.

## Inputs

- Target code, behavior, or bug to cover.
- Optional scope: validator, service, mapper, controller/result mapping, repository, or SharedKernel type.

## Required context

- Read `AGENTS.md` first.
- Read `ACTIVE_CONTEXT.md` if it exists.
- Inspect the relevant code, tests, configuration, and docs before making claims or edits.
- Treat the current codebase as more authoritative than this workflow file if they disagree.
- Separate current implementation from desired future state.
- If `ACTIVE_CONTEXT.md` does not exist, continue without failing.

## Workflow

1. Identify the smallest useful test scope before writing code.
2. Read nearby tests and the owning implementation to match naming and test style.
3. Choose the simplest suitable test type:
	- SharedKernel public behavior tests in `Rasmus.SharedKernel.Tests`
	- Application validator or service tests in `media-vault-app.Tests`
	- API or integration-style coverage only when unit-level coverage is not enough
4. Prefer behavior tests over implementation-detail tests.
5. Cover meaningful success, failure, and edge-case paths.
6. For regression work, reproduce the bug first before locking in the fix.
7. Do not duplicate production logic inside assertions.
8. Prefer manual fakes over adding mocking libraries unless a fake would become harder to understand.
9. Run the narrowest relevant test command after editing.

## Repo-specific checks

- Existing tests use xUnit.
- Existing naming trends follow `Method_WhenCondition_ShouldExpectedOutcome`.
- Validator and Result Pattern coverage are stronger than service and integration coverage.
- There is currently no frontend test suite, so frontend testing work should be proposed deliberately rather than assumed.

## Test quality checklist

Good tests should be:

- focused on observable behavior
- readable by a junior developer
- stable under harmless refactors
- clear about the scenario and expected outcome
- placed near the relevant existing test style
- useful for preventing regressions

## Output format

1. Selected test scope
2. Existing nearby test style
3. Test cases to add
4. Test implementation
5. Validation command and result
6. Remaining coverage gaps

## Validation fallback

If validation commands cannot be run, state exactly:

- what changed
- what was not validated
- which command should be run manually

