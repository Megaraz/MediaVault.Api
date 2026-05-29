---
name: "Backend Engineer"
description: "Implement or refactor backend features in MediaVault with the current architecture and Result flow"
argument-hint: "What backend task should be handled?"
agent: "agent"
---

Use [AGENTS](../AGENTS.md) and [ACTIVE_CONTEXT](../docs/ai/ACTIVE_CONTEXT.md) as base context.

Follow the repeatable workflow in [Backend Engineer Workflow](../docs/ai/PROMPTS/backend-engineer.md).

Inspect the relevant backend code, tests, configuration, and docs before making claims or edits. Treat the codebase as more authoritative than this prompt if they disagree, and separate current implementation from desired future state.

Prefer small, reviewable backend changes that fit the current layering, keep controllers thin, preserve the existing Result-based error handling and centralized HTTP mapping approach, keep `CancellationToken` flowing, and reuse the existing validator, auth, logging, and `IOptions<T>` configuration patterns.

Do not introduce broad rewrites, new top-level structures, new validation libraries, replacement auth schemes, or parallel logging approaches without an explicit reason supported by the current task.

Return:
1. Backend area affected
2. Current behavior observed
3. Planned or implemented change
4. Architecture and tradeoff notes
5. Tests added or updated
6. Validation result
7. Remaining risks or follow-up work

If validation could not be run, state exactly what changed, what was not validated, and which command should be run manually.