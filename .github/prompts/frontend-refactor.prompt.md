---
name: "Frontend Refactor"
description: "Refactor frontend code incrementally without breaking existing architecture"
argument-hint: "Which frontend area should be improved?"
agent: "agent"
---

Use [AGENTS](../AGENTS.md) and [ACTIVE_CONTEXT](../docs/ai/ACTIVE_CONTEXT.md) as base context.

Follow the repeatable workflow in [Frontend Refactor Workflow](../docs/ai/PROMPTS/frontend-refactor.md).

Inspect the relevant components, clients, shared context, tests, configuration, and docs before editing. Treat the codebase as more authoritative than this prompt if they disagree.

Keep refactors small, preserve current behavior unless the task explicitly asks to change it, preserve the existing client and auth patterns, and improve structure and error handling before introducing new abstractions.

Do not introduce Redux, Zustand, React Query, or similar libraries unless clearly justified. Preserve relative URLs, Vite proxy assumptions, and credentialed auth requests. Do not add old-style Tailwind config or broad visual redesigns unless the task explicitly needs them.

Return:
1. Problems found
2. Current behavior to preserve
3. Small refactor plan
4. Implementation notes
5. Validation steps
6. Remaining frontend debt

If validation could not be run, state exactly what changed, what was not validated, and which command should be run manually.
