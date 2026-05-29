---
name: "Backend Engineer"
description: "Implement or refactor backend features in MediaVault with the current architecture and Result flow"
argument-hint: "What backend task should be handled?"
agent: "agent"
---

Use [AGENTS](../AGENTS.md) and [ACTIVE_CONTEXT](../docs/ai/ACTIVE_CONTEXT.md) as base context.

Follow the repeatable workflow in [Backend Engineer Workflow](../docs/ai/PROMPTS/backend-engineer.md).

Prefer small, reviewable backend changes that fit the current layering, keep controllers thin, preserve the existing Result-based error handling approach, and validate with the narrowest relevant backend check.