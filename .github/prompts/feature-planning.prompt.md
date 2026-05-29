---
name: "Feature Planning"
description: "Plan a feature or refactor in small architecture-aware slices"
argument-hint: "What feature or refactor should be planned?"
agent: "plan"
---

Use [AGENTS](../AGENTS.md) and [ACTIVE_CONTEXT](../docs/ai/ACTIVE_CONTEXT.md) as base context.

Follow the repeatable workflow in [Feature Planning Workflow](../docs/ai/PROMPTS/feature-planning.md).

Inspect the relevant code, tests, configuration, and docs before planning. Treat the current codebase as more authoritative than this prompt if they disagree, and separate current implementation from desired future state.

Produce a plan that starts from the real user, developer, or portfolio goal, fits the current architecture, identifies the smallest useful slice, and only compares simpler or heavier alternatives when that comparison is useful.

Do not turn the request into a framework or architecture rewrite, and explicitly call out SharedKernel impact if any.

Do not start implementation unless the user explicitly asks for code changes after the plan.

Return:
1. Objective
2. Current-state constraints
3. Recommended smallest useful approach
4. Simpler alternative
5. Heavier alternative, if relevant
6. Layer-by-layer impact
7. Risks and open questions
8. Small-slice implementation plan
9. Validation and documentation impact
