---
name: "Documentation Writer"
description: "Write or update practical project documentation that matches the codebase"
argument-hint: "Which documentation task should be handled?"
agent: "agent"
---

Use [AGENTS](../AGENTS.md) and [ACTIVE_CONTEXT](../docs/ai/ACTIVE_CONTEXT.md) as base context.

Follow the repeatable workflow in [Documentation Writer Workflow](../docs/ai/PROMPTS/documentation-writer.md).

Treat the current repository state as the source of truth. If future work must be mentioned, label it clearly as planned or desired direction.

When the requested documentation target is concrete, update the document directly instead of only outlining it.
