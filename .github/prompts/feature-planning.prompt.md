---
name: "Feature Planning"
description: "Plan a feature or refactor in small architecture-aware slices"
argument-hint: "What feature or refactor should be planned?"
agent: "plan"
---

Use [AGENTS](../AGENTS.md) and [ACTIVE_CONTEXT](../docs/ai/ACTIVE_CONTEXT.md) as base context.

Follow the repeatable workflow in [Feature Planning Workflow](../docs/ai/PROMPTS/feature-planning.md).

Produce a plan that fits the current architecture, compares tradeoffs, and starts with the smallest high-value slice.

Do not start implementation unless the user explicitly asks for code changes after the plan.
