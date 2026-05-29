# ACTIVE_CONTEXT.md - MediaVault

## Purpose

This file is a short current-state companion to `AGENTS.md`.

Read this together with `AGENTS.md` before proposing larger refactors, new architecture, broad documentation plans, or new AI workflow files.

## Current focus

The school and exam phase is complete.

The current phase is re-orientation, stabilization, portfolio polish, public-repo cleanup, and personal-use hardening.

The immediate focus is a fresh review of the current codebase before major new implementation work.

## Current priorities

1. Re-orient with a fresh whole-repo code review and architecture review.
2. Finish and harden SharedKernel tests.
3. Document SharedKernel public behavior.
4. Fill the major documentation gaps across the repo.
5. Add missing backend tests across MediaVault.
6. Improve frontend structure and error handling.
7. Prepare the repo for public release with a stronger README, screenshots or demo media, and recruiter-friendly presentation.
8. Improve account and auth flows after the stabilization and documentation work is in better shape.

## Current concerns

- The repo needs a fresh review pass because active work paused for a while.
- SharedKernel should remain reusable outside MediaVault.
- The frontend is functional but still needs structural cleanup and stronger error handling.
- Account management is still thin and likely needs a real account page plus user-update flow improvements.
- Logging and error handling are in a better place than before, but observability is still early-stage.
- The repository should become easier for recruiters and reviewers to understand quickly.
- Several important documentation files are still missing or empty.
- The repo should move toward business-level quality without drifting into unnecessary abstraction or overengineering.

## Later-phase priorities

- Improve logging and observability after the current review, documentation, and testing push.
- Add caching only where the real usage patterns justify it.
- Prepare deployment once the repo and app are stable enough to represent well in public.

## Instructions for AI assistants

- Always read this file together with `AGENTS.md` before suggesting larger changes.
- Treat the actual codebase as the source of truth over older plans or generic advice.
- Prefer small, reviewable improvements over broad rewrites.
- Update this file when the current priorities, risks, or phase of the project materially change.
