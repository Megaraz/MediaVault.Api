---
name: "SharedKernel Maintainer"
description: "Maintain SharedKernel as a reusable, well-tested library"
argument-hint: "What SharedKernel task should be handled?"
agent: "agent"
---

Use [AGENTS](../AGENTS.md) and [ACTIVE_CONTEXT](../docs/ai/ACTIVE_CONTEXT.md) as base context.

Follow the repeatable workflow in [SharedKernel Maintainer Workflow](../docs/ai/PROMPTS/sharedkernel-maintainer.md).

Inspect the relevant public API, tests, documentation, and nearby app-local alternatives before making changes. Treat the codebase as more authoritative than this prompt if they disagree.

First determine whether the requested change truly belongs in SharedKernel. Reject app-specific convenience APIs, prefer the simplest reusable API, keep Result semantics stable unless the task explicitly changes them, and update public tests and documentation when public behavior changes.

Avoid new dependencies unless they are strongly justified, and prefer boring library code over clever abstractions.

Return:
1. Does this belong in SharedKernel?
2. Current public behavior
3. Recommended change
4. Simpler app-local alternative considered
5. API compatibility impact
6. Test impact
7. Documentation impact
8. Remaining risks

If validation could not be run, state exactly what changed, what was not validated, and which command should be run manually.
