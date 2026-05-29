# Code Review Workflow

Use this workflow for reviewing a specific file, a changed slice, or a PR-sized change.

## Goal

Find the highest-value issues first: correctness, regressions, security, layering mistakes, weak error handling, and long-term maintainability risks.

## Inputs

- Review target: file, folder, diff, feature slice, or PR-equivalent scope.
- Optional context: intended behavior, known bug, related issue, or risk area.

## Workflow

1. Read `AGENTS.md` and `ACTIVE_CONTEXT.md` first.
2. Establish the review scope before commenting. If the target is broad, anchor on the owning code path rather than skimming the whole repo.
3. Inspect the current implementation and nearby tests before making claims.
4. Review with bug-risk-first priority:
	- correctness and broken assumptions
	- security and auth mistakes
	- result/error handling consistency
	- architecture and layering violations
	- async and cancellation-token handling
	- test coverage gaps
	- maintainability and naming problems
5. Validate important findings with evidence from the code, tests, or runtime behavior when possible.
6. Deliver findings first, ordered by severity, with concise explanations and concrete fixes.

## Repo-specific checks

- Backend: keep business logic out of controllers and keep HTTP mapping centralized through the result-mapping flow.
- SharedKernel: reject MediaVault-specific assumptions in reusable code.
- Frontend: watch for duplicated state, weak error handling, hardcoded API URLs, and missing authenticated request credentials.
- Documentation changes: reject inflated claims that the repo does not support.

## Output format

1. Findings
2. Open questions or assumptions
3. Short summary or residual risk note

If no findings are discovered, say so explicitly and mention remaining risk or testing gaps.
