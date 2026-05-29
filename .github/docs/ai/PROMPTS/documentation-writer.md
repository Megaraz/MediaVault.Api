# Documentation Writer Workflow

Use this workflow when creating or updating README content, setup docs, architecture docs, testing docs, API docs, deployment docs, or ADRs.

## Goal

Produce practical documentation that matches the real codebase, explains tradeoffs, and is understandable to both recruiters and developers.

## Inputs

- Target document or documentation area.
- Optional audience: recruiter, contributor, maintainer, or future self.
- Optional task: create new doc, update stale doc, or rewrite for clarity.

## Required context

- Read `AGENTS.md` first.
- Read `ACTIVE_CONTEXT.md` if it exists.
- Inspect the relevant code, tests, configuration, and docs before making claims or edits.
- Treat the current codebase as more authoritative than this workflow file if they disagree.
- Separate current implementation from desired future state.
- If `ACTIVE_CONTEXT.md` does not exist, continue without failing.

## Workflow

1. Inspect the relevant code, config, commands, and existing docs before writing.
2. Confirm setup, build, and test commands against repo files before documenting them.
3. Separate current behavior from future direction. Label planned work as planned work.
4. Explain why key choices were made and what tradeoffs were accepted.
5. Keep the tone honest: portfolio-quality, production-inspired, and practical. Avoid inflated claims.
6. Public-facing docs should quickly explain what the app is, what it demonstrates, and how to run it.
7. Internal docs can go deeper on implementation details and tradeoffs.

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

## Repo-specific checks

- Documentation must not overclaim.
- Do not describe planned features as implemented.
- Match the actual codebase and current repo maturity.
- Mention that the project began as a solo school project and was later polished when that context helps.
- Include screenshots or GIF suggestions when they would improve understanding, especially in README work.
- Update docs directly when the task is concrete instead of only producing an outline.

## Documentation quality checklist

A good public-facing document should answer:

- What is MediaVault?
- What works today?
- Why does this project exist?
- What architecture and patterns does it demonstrate?
- How do I run it locally?
- What external APIs or secrets are required?
- What tests exist?
- What is planned but not done yet?
- Why is this credible as a portfolio project?

## Output format

1. Document target and audience
2. Current-state facts confirmed
3. Draft or updated documentation
4. Future-work labels, if any
5. Commands or references that should be manually verified
