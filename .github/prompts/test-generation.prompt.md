---
name: "Test Generation"
description: "Generate focused tests that match existing repo patterns"
argument-hint: "What code or behavior needs tests?"
agent: "agent"
---

Use [AGENTS](../AGENTS.md) and [ACTIVE_CONTEXT](../docs/ai/ACTIVE_CONTEXT.md) as base context.

Follow the repeatable workflow in [Test Generation Workflow](../docs/ai/PROMPTS/test-generation.md).

Choose the smallest useful test scope, match existing test style, and run the narrowest relevant validation after editing.