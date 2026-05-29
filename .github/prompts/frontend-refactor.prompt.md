---
name: "Frontend Refactor"
description: "Refactor frontend code incrementally without breaking existing architecture"
argument-hint: "Which frontend area should be improved?"
agent: "agent"
---

Use [AGENTS](../AGENTS.md) and [ACTIVE_CONTEXT](../docs/ai/ACTIVE_CONTEXT.md) as base context.

Follow the repeatable workflow in [Frontend Refactor Workflow](../docs/ai/PROMPTS/frontend-refactor.md).

Keep refactors small, preserve the existing client and auth patterns, and improve structure and error handling before introducing new abstractions.
