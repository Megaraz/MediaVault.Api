# LESSONS_LEARNED.md - MediaVault

## Purpose

This file records durable lessons learned while evolving the repository and its AI infrastructure.

Update it when a lesson is verified by the codebase, tests, or repeated experience and is likely to help future work.

## Update rules

- Keep entries short, factual, and useful.
- Record lessons, gotchas, and clarified conventions, not temporary task logs.
- Prefer updating or replacing outdated lessons over stacking contradictions.
- If a lesson only matters to AI workflow internals, also mirror the concise version into repo memory.

## Current lessons

- 2026-05-29: `AGENTS.md` is the single root instruction file for this repo. Do not add an overlapping `copilot-instructions.md`.
- 2026-05-29: Runnable workspace prompt files live in `.github/prompts/*.prompt.md`, while the detailed reusable workflow notes live in `.github/docs/ai/PROMPTS/*.md`.
- 2026-05-29: Frontend authentication depends on cookie-based requests with `credentials: "include"` and the Vite proxy setup. Hardcoded dev API URLs would be a regression.
- 2026-05-29: Tailwind uses v4 through `@tailwindcss/vite` and `@import "tailwindcss"` in `src/index.css`; there is no current `tailwind.config.js` requirement.
- 2026-05-29: Backend expected-outcome flows are built around `Result` and centralized HTTP response mapping rather than controller-by-controller status handling.
