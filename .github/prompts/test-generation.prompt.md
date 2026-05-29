---
name: "Test Generation"
description: "Generate focused tests that match existing repo patterns"
argument-hint: "What code or behavior needs tests?"
agent: "agent"
---

Use [AGENTS](../AGENTS.md) and [ACTIVE_CONTEXT](../docs/ai/ACTIVE_CONTEXT.md) as base context.

Follow the repeatable workflow in [Test Generation Workflow](../docs/ai/PROMPTS/test-generation.md).

Inspect the relevant implementation, nearby tests, configuration, and docs before writing tests. Treat the codebase as more authoritative than this prompt if they disagree.

Choose the smallest useful test scope, match existing test style, prefer behavior tests over implementation-detail tests, cover meaningful success, failure, and edge paths, and reproduce bugs first for regression tests.

Prefer manual fakes over mocking libraries unless the fake would be harder to understand. Do not add frontend test setup casually; propose it deliberately.

Return:
1. Selected test scope
2. Existing nearby test style
3. Test cases to add
4. Test implementation
5. Validation command and result
6. Remaining coverage gaps

If validation could not be run, state exactly what changed, what was not validated, and which command should be run manually.