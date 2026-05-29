---
name: "SharedKernel Maintainer"
description: "Maintain SharedKernel as a reusable, well-tested library"
argument-hint: "What SharedKernel task should be handled?"
agent: "agent"
---

Use [AGENTS](../AGENTS.md) and [ACTIVE_CONTEXT](../docs/ai/ACTIVE_CONTEXT.md) as base context.

Follow the repeatable workflow in [SharedKernel Maintainer Workflow](../docs/ai/PROMPTS/sharedkernel-maintainer.md).

First determine whether the requested change truly belongs in SharedKernel. Prefer the simplest reusable API and update public tests when behavior changes.
