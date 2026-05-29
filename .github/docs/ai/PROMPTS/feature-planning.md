# Feature Planning Workflow

Use this workflow when planning a new feature, a refactor, or a significant improvement before implementation starts.

## Goal

Produce an incremental, architecture-aware plan that solves the actual problem without overengineering.

## Inputs

- Feature, bug, refactor, or capability to plan.
- Optional constraints: deadline, scope limit, no new dependencies, or preferred layer.

## Workflow

1. Read `AGENTS.md` and `ACTIVE_CONTEXT.md` first.
2. Understand the real user or business goal before proposing structure.
3. Identify the owning layer or code path in the existing architecture.
4. Propose the smallest viable approach that fits current patterns.
5. Compare that approach with one simpler alternative and one heavier alternative when tradeoffs matter.
6. Call out impact on:
	- Domain model
	- Application services and validators
	- Infrastructure and persistence
	- API surface and auth
	- frontend clients and UI state
	- tests and documentation
7. Break the work into reviewable slices with a clear first step.

## Repo-specific planning rules

- Respect the current layered architecture and Result Pattern flow.
- Preserve type-specific media entry patterns unless there is a strong reason to consolidate.
- Do not introduce large frontend state libraries or new abstractions by default.
- Treat SharedKernel changes as library design work, not app-only convenience changes.

## Output format

1. Objective
2. Current-state constraints
3. Recommended approach
4. Alternatives and tradeoffs
5. Risks and open questions
6. Small-slice implementation plan
7. Validation and documentation impact
