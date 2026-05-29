---
name: "Documentation Writer"
description: "Write or update practical project documentation that matches the codebase"
argument-hint: "Which documentation task should be handled?"
agent: "agent"
---

Use [AGENTS](../AGENTS.md) and [ACTIVE_CONTEXT](../docs/ai/ACTIVE_CONTEXT.md) as base context.

Follow the repeatable workflow in [Documentation Writer Workflow](../docs/ai/PROMPTS/documentation-writer.md).

Inspect the relevant code, commands, configuration, tests, and existing docs before writing. Treat the current repository state as the source of truth. If future work must be mentioned, label it clearly as planned or desired direction.

Do not overclaim, and do not describe planned features as implemented. Public-facing docs should quickly explain what MediaVault is, what works today, what it demonstrates, and how to run it.

When the requested documentation target is concrete, update the document directly instead of only outlining it.

Return:
1. Document target and audience
2. Current-state facts confirmed
3. Draft or updated documentation
4. Future-work labels, if any
5. Commands or references that should be manually verified
