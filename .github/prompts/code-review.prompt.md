---
name: "Code Review"
description: "Review code with a bug-risk-first, architecture-aware workflow"
argument-hint: "What should be reviewed?"
agent: "agent"
---

Use [AGENTS](../AGENTS.md) and [ACTIVE_CONTEXT](../docs/ai/ACTIVE_CONTEXT.md) as base context.

Follow the repeatable workflow in [Code Review Workflow](../docs/ai/PROMPTS/code-review.md).

Use the user's message, selected files, or nearby changes as the review target. Inspect the relevant code, nearby tests, configuration, and docs before making claims. Treat the codebase as more authoritative than this prompt if they disagree.

Prefer fewer high-confidence findings over many vague comments. Do not claim something is broken without checking nearby code and tests when possible.

For each finding, include severity, location, issue, why it matters, and a suggested fix.

Return:
1. Findings, ordered by severity
2. Open questions or assumptions
3. Tests or validation reviewed
4. Short residual risk note

If no findings are discovered, say so explicitly and mention remaining risk or testing gaps.
