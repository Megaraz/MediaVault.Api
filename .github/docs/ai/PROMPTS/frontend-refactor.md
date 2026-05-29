# Frontend Refactor Workflow

Use this workflow when cleaning up React and TypeScript code in the frontend without changing the product direction.

## Goal

Improve structure, readability, error handling, and maintainability through small refactors that fit the current frontend architecture.

## Inputs

- Target page, component, client, hook, or frontend feature slice.
- Optional concern: state duplication, error handling, styling cleanup, loading flow, or type safety.

## Workflow

1. Read `AGENTS.md` and `ACTIVE_CONTEXT.md` first.
2. Inspect the owning page, child components, API client usage, and shared context before editing.
3. Identify the smallest useful refactor that improves clarity or reliability.
4. Prefer these improvements first:
	- clearer component boundaries
	- reduced duplicated state
	- better loading, empty, and error states
	- stronger TypeScript types
	- moving raw API calls into `src/Clients`
5. Preserve the current auth and networking approach:
	- relative URLs
	- Vite proxy
	- credentialed requests for authenticated flows
6. Avoid introducing new libraries unless the current structure clearly cannot support the change.

## Repo-specific checks

- `UserContext` is the current auth state anchor.
- `src/Clients` is the existing API access layer.
- Tailwind CSS v4 is already configured; do not add old-style Tailwind config unless truly needed.
- Some existing files are transitional or prototype-quality. Improve them when touched, but do not copy their rough edges into new work.

## Output format

1. Problems found
2. Small refactor plan
3. Implementation notes
4. Validation steps
