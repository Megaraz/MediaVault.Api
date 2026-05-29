# Feature Planning Workflow

Use this workflow when planning a new feature, a refactor, or a significant improvement before implementation starts.

## Goal

Produce an incremental, architecture-aware plan that solves the actual problem without overengineering.

## Inputs

- Feature, bug, refactor, or capability to plan.
- Optional constraints: deadline, scope limit, no new dependencies, or preferred layer.

## Required context

- Read `AGENTS.md` first.
- Read `ACTIVE_CONTEXT.md` if it exists.
- Inspect the relevant code, tests, configuration, and docs before making claims.
- Treat the current codebase as more authoritative than this workflow file if they disagree.
- Separate current implementation from desired future state.
- If `ACTIVE_CONTEXT.md` does not exist, continue without failing.

## Workflow

1. Start from the real user, developer, or portfolio goal before proposing structure.
2. Identify the owning layer or code path in the existing architecture.
3. Identify the smallest useful slice that delivers real value.
4. Compare the recommended approach with one simpler alternative and one heavier alternative only when useful.
5. Call out impact on:
	- Domain model
	- Application services and validators
	- Infrastructure and persistence
	- API surface and auth
	- frontend clients and UI state
	- SharedKernel, if affected
	- tests and documentation
6. Break the work into reviewable slices with a clear first step.

## Repo-specific planning rules

- Respect the current layered architecture and Result Pattern flow.
- Preserve type-specific media entry patterns unless there is a strong reason to consolidate.
- Do not introduce large frontend state libraries or new abstractions by default.
- Treat SharedKernel changes as library design work, not app-only convenience changes.
- Do not turn every feature into a framework or architecture rewrite.
- Do not create new top-level folders, projects, architectural layers, libraries, or naming schemes without first checking whether an existing convention already fits.

## Output format

1. Objective
2. Current-state constraints
3. Recommended smallest useful approach
4. Simpler alternative
5. Heavier alternative, if relevant
6. Layer-by-layer impact
7. Risks and open questions
8. Small-slice implementation plan
9. Validation and documentation impact
