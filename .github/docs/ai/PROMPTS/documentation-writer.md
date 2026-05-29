# Documentation Writer Workflow

Use this workflow when creating or updating README content, setup docs, architecture docs, testing docs, API docs, deployment docs, or ADRs.

## Goal

Produce practical documentation that matches the real codebase, explains tradeoffs, and is understandable to both recruiters and developers.

## Inputs

- Target document or documentation area.
- Optional audience: recruiter, contributor, maintainer, or future self.
- Optional task: create new doc, update stale doc, or rewrite for clarity.

## Workflow

1. Read `AGENTS.md` and `ACTIVE_CONTEXT.md` first.
2. Inspect the relevant code, config, commands, and existing docs before writing.
3. Separate current behavior from future direction. Label planned work as planned work.
4. Explain why key choices were made and what tradeoffs were accepted.
5. Include setup or usage commands only after confirming they match the repo.
6. Keep the tone honest: portfolio-quality, production-inspired, and practical. Avoid inflated claims.
7. If the document is public-facing, optimize for fast comprehension and credibility.

## Repo-specific documentation targets

- Root README
- setup instructions
- architecture overview
- database overview
- external API documentation
- SharedKernel documentation
- testing guide
- deployment guide
- ADRs for major architecture decisions

## Output expectations

- Match the actual codebase and current repo maturity.
- Mention that the project began as a solo school project and was later polished when that context helps.
- Include screenshots or GIF suggestions when they would improve understanding, especially in README work.
- Update docs directly when the task is concrete instead of only producing an outline.
