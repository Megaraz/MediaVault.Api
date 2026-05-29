# Test Generation Workflow

Use this workflow when generating or updating tests for backend or SharedKernel code.

## Goal

Add focused tests that match existing patterns, prove behavior, and keep the scope as small as possible.

## Inputs

- Target code, behavior, or bug to cover.
- Optional scope: validator, service, mapper, controller/result mapping, repository, or SharedKernel type.

## Workflow

1. Read `AGENTS.md` and `ACTIVE_CONTEXT.md` first.
2. Identify the smallest useful test scope before writing code.
3. Read nearby tests and the owning implementation to match naming and test style.
4. Choose the simplest suitable test type:
	- SharedKernel public behavior tests in `Rasmus.SharedKernel.Tests`
	- Application validator or service tests in `media-vault-app.Tests`
	- API or integration-style coverage only when unit-level coverage is not enough
5. Cover success, validation failure, error, and edge-case paths that matter to the behavior.
6. Prefer manual fakes over adding mocking libraries unless a fake would become harder to understand.
7. Run the narrowest relevant test command after editing.

## Repo-specific checks

- Existing tests use xUnit.
- Existing naming trends follow `Method_WhenCondition_ShouldExpectedOutcome`.
- Validator and Result Pattern coverage are stronger than service and integration coverage.
- There is currently no frontend test suite, so frontend testing work should be proposed deliberately rather than assumed.

## Output format

1. Selected test scope
2. Test cases to add
3. Test implementation
4. Validation run and result
5. Remaining coverage gaps

