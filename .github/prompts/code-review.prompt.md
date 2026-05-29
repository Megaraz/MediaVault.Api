---
name: "Code Review"
description: "Review code with a bug-risk-first, architecture-aware workflow"
argument-hint: "What should be reviewed?"
agent: "agent"
---

Use [AGENTS](../AGENTS.md) and [ACTIVE_CONTEXT](../docs/ai/ACTIVE_CONTEXT.md) as base context.

Follow the repeatable workflow in [Code Review Workflow](../docs/ai/PROMPTS/code-review.md).

Use the user's message, selected files, or nearby changes as the review target.

Return findings first, ordered by severity, with file references. Then provide open questions or assumptions, followed by a short summary or residual risk note.
