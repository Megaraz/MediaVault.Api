# ROADMAP.md - MediaVault

## Purpose

This is the current working roadmap for MediaVault.

It is not a promise or a sprint board. It is the best current ordering of the major workstreams based on the actual codebase, the current repo state, and the last known direction of the project.

Read this together with `AGENTS.md` and `ACTIVE_CONTEXT.md`.

## Current phase

MediaVault is past the school and exam phase.

The current phase is stabilization, portfolio polish, public-repo preparation, and personal-use hardening.

## Recently completed groundwork

These are the most relevant recent foundations already in place:

- SharedKernel review and test work has already started and should be continued rather than abandoned.
- A basic file-based error logger was added.
- Certain backend failures, especially database-related ones, now flow through that logging approach.
- `Result`, `DatabaseError`, and `HttpError` were refactored toward more realistic application error handling, instead of only `ValidationError` carrying most of the value.

## Roadmap principles

- Prefer finishing and hardening existing work before adding shiny new features.
- Favor practical improvements that make the repo stronger for both portfolio review and real personal use.
- Keep the architecture understandable and avoid overengineering.
- Treat documentation, testing, and maintainability as product work, not optional cleanup.
- Separate current reality from future direction when documenting plans.

## Working roadmap

### 1. Re-orient and review the current codebase

This is the immediate top-level step.

The repo has changed enough that a fresh code review and architecture review should come before major new implementation work.

Focus:

- review backend, frontend, SharedKernel, and AI infrastructure with current standards
- identify correctness issues, weak abstractions, stale code, and documentation gaps
- turn fuzzy concerns into concrete follow-up tasks

Why this comes first:

- it reduces the chance of building on top of weak assumptions
- it makes the rest of the roadmap more deliberate instead of reactive

### 2. Finish SharedKernel hardening

This is still the most valuable concrete technical priority inside the broader review-and-stabilize phase.

Focus:

- finish and strengthen SharedKernel tests
- document SharedKernel public behavior
- review naming, public API clarity, and reusability boundaries
- keep MediaVault-specific assumptions out of the library

Why this is high priority:

- SharedKernel is the most reusable part of the repo
- it already has momentum from recent work
- it raises the quality bar for the rest of the solution

### 3. Fill the major documentation gaps

Once the current architecture and SharedKernel direction are clearer, the next priority is practical documentation.

Focus:

- root README
- setup instructions
- architecture overview
- database overview
- external API documentation
- SharedKernel documentation
- testing guide
- deployment guide
- ADRs for major architectural decisions

Why this matters now:

- the repo is aiming toward public visibility and recruiter review
- several important docs are still missing or empty
- documentation will clarify what the project currently is, not just what it may become later

### 4. Increase backend confidence with tests

After the review and documentation pass, the next major technical track is broader backend test coverage.

Focus:

- missing unit tests in Application and SharedKernel-adjacent behavior
- validator tests where gaps remain
- result and mapper tests where behavior matters
- integration tests for important API, repository, and database flows
- auth-related behavior that deserves stronger confidence

Why this comes before major feature expansion:

- it makes future refactoring safer
- it helps separate real defects from architecture anxiety
- it improves public credibility when the repo goes public

### 5. Clean up the frontend and strengthen UX basics

The frontend is functional, but it still needs structural cleanup and better resilience.

Focus:

- clearer component boundaries
- less duplicated state
- stronger loading, empty, and error states
- cleaner API client usage
- better overall maintainability without forcing enterprise-scale frontend patterns

Why this matters:

- the frontend is part of the portfolio surface
- rough UI structure and weak error handling are easy review liabilities
- it should become easier to maintain before adding more user-facing features

### 6. Prepare the repo for public release and portfolio use

After the codebase is more stable and documented, the next step is to make the public-facing presentation strong.

Focus:

- improve the public README
- add screenshots, GIFs, or demo media
- explain the project clearly for recruiters and junior-dev portfolio value
- present architecture and tradeoffs honestly
- make the repo understandable quickly by someone seeing it for the first time

Desired outcome:

- a public repository that is credible, readable, and easy to evaluate

### 7. Improve account and authentication flows

This is a real product gap, but not the first thing to do before stabilization and documentation.

Focus:

- add or improve an account page
- let users view and update their own account information cleanly
- harden the current auth flow where needed
- evaluate optional external sign-in later, such as Google sign-in, only if it genuinely improves the product and portfolio story

Why this is not first:

- the current cookie-based auth flow already exists
- better stability, docs, and test coverage come first
- external sign-in should be a deliberate product decision, not a resume-driven bolt-on

### 8. Improve observability and logging

The current logging work is a good first step, but it is still early.

Focus:

- clean up logging boundaries
- improve structured logging where it adds value
- make error investigation easier
- decide what should be logged, where, and why
- consider correlation or traceability improvements later if they solve a real problem

### 9. Add caching where it is justified

Caching is not the first lever to pull, but it is a sensible later-stage optimization.

Focus:

- external media API calls
- repeated lookups with real cost or rate-limit pressure
- minimal in-memory caching first
- clear invalidation and freshness rules

Guardrail:

- do not add caching just because it sounds production-like

### 10. Prepare deployment and live-demo readiness

Deployment work should follow once the repo is stable enough that hosting it will not immediately create churn.

Focus:

- environment-variable and secret handling
- SQL hosting choices
- frontend and backend hosting shape
- Docker only if it simplifies the path
- GitHub Actions where they reduce manual friction
- deployment documentation

End goal:

- make the repo public with a good README and demo support
- publish a live demo only when the app is stable enough to represent the project well

## Not the current priority

These may become important later, but they should not distract from the current roadmap:

- major new media features before the current foundations are hardened
- large-scale architecture rewrites
- enterprise-style infrastructure for its own sake
- new packages or patterns without a concrete problem to solve

## Revisit trigger

This roadmap should be updated when one of these happens:

- a major code review changes the perceived priority order
- SharedKernel hardening is substantially complete
- the repo is ready to go public
- deployment work becomes active instead of aspirational
- a new product priority becomes clearly more important than stabilization

