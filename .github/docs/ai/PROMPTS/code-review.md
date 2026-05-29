# Code Review Workflow

Use this workflow for reviewing a specific file, a changed slice, or a PR-sized change.

## Goal

Find the highest-value issues first: correctness, regressions, security, layering mistakes, weak error handling, and long-term maintainability risks.

## Inputs

- Review target: file, folder, diff, feature slice, or PR-equivalent scope.
- Optional context: intended behavior, known bug, related issue, or risk area.

## Required context

- Read `AGENTS.md` first.
- Read `ACTIVE_CONTEXT.md` if it exists.
- Inspect the relevant code, tests, configuration, and docs before making claims.
- Treat the current codebase as more authoritative than this workflow file if they disagree.
- Separate current implementation from desired future state.
- If `ACTIVE_CONTEXT.md` does not exist, continue without failing.

## Workflow

1. Establish the review scope before commenting. If the target is broad, anchor on the owning code path rather than skimming the whole repo.
2. Inspect the current implementation and nearby tests before making claims.
3. Review with bug-risk-first priority:
	- correctness and broken assumptions
	- security and auth mistakes
	- result/error handling consistency
	- architecture and layering violations
	- async and cancellation-token handling
	- test coverage gaps
	- maintainability and naming problems
4. Prefer fewer high-confidence findings over many vague comments.
5. Do not claim something is broken without checking nearby code and tests.
6. Validate important findings with evidence from the code, tests, or runtime behavior when possible.
7. Separate must-fix issues from could-improve issues.

## Repo-specific checks

- Backend: keep business logic out of controllers and keep HTTP mapping centralized through the result-mapping flow.
- SharedKernel: reject MediaVault-specific assumptions in reusable code.
- Frontend: watch for duplicated state, weak error handling, hardcoded API URLs, and missing authenticated request credentials.
- Documentation changes: reject inflated claims that the repo does not support.

## Finding format

For each finding, include:

- Severity: Critical, High, Medium, Low, or Nit
- Location
- Issue
- Why it matters
- Suggested fix

## Output format

1. Findings, ordered by severity
2. Open questions or assumptions
3. Tests or validation reviewed
4. Short residual risk note

If no findings are discovered, say so explicitly and mention remaining risk or testing gaps.
