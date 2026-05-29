# Frontend Refactor Workflow

Use this workflow when cleaning up React and TypeScript code in the frontend without changing the product direction.

## Goal

Improve structure, readability, error handling, and maintainability through small refactors that fit the current frontend architecture.

## Inputs

- Target page, component, client, hook, or frontend feature slice.
- Optional concern: state duplication, error handling, styling cleanup, loading flow, or type safety.

## Required context

- Read `AGENTS.md` first.
- Read `ACTIVE_CONTEXT.md` if it exists.
- Inspect the relevant code, tests, configuration, and docs before making claims or edits.
- Treat the current codebase as more authoritative than this workflow file if they disagree.
- Separate current implementation from desired future state.
- If `ACTIVE_CONTEXT.md` does not exist, continue without failing.

## Workflow

1. Inspect the owning page, child components, API client usage, and shared context before editing.
2. Confirm the current behavior that should be preserved unless the task explicitly asks to change it.
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
- Preserve behavior unless the task explicitly asks to change it.
- Do not introduce Redux, Zustand, React Query, or similar libraries unless clearly justified.
- Tailwind CSS v4 is already configured; do not add old-style Tailwind config unless truly needed.
- Avoid broad visual redesigns unless requested.
- Some existing files are transitional or prototype-quality. Improve them when touched, but do not copy their rough edges into new work.
- Do not create new top-level folders, projects, architectural layers, libraries, or naming schemes without first checking whether an existing convention already fits.
- Do not delete files, remove public APIs, rename projects, or perform broad formatting-only changes unless explicitly asked or clearly required by the current task.

## Output format

1. Problems found
2. Current behavior to preserve
3. Small refactor plan
4. Implementation notes
5. Validation steps
6. Remaining frontend debt

## Validation fallback

If validation commands cannot be run, state exactly:

- what changed
- what was not validated
- which command should be run manually
