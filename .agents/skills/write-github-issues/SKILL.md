---
name: write-github-issues
description: Create, refine, or review professional GitHub issues and parent/subissue hierarchies for MediaVault. Use when Codex drafts or publishes implementation issues, turns plans or backlog items into AI-executable tasks, prepares sprint or milestone issues, or improves issue specifications for portfolio-quality public work.
---

# Write GitHub Issues

Create issues that are readable by recruiters and developers and executable by an AI agent without hidden context.

## Build context first

1. Read the nearest applicable `AGENTS.md`.
2. Inspect the current code, tests, manifests, and relevant plans or ADRs.
3. Read the parent issue and linked issues when working in an existing hierarchy.
4. Search existing open and closed issues for duplicates or superseded work.
5. Separate current behavior, approved work, and future direction. Do not present planned behavior as implemented.

Use repository-relative paths and durable GitHub links. Refer to symbols or API routes when they are more stable than line numbers.

## Choose the issue boundary

- Give one implementation issue one coherent, independently verifiable outcome.
- Use a parent issue for a sprint, milestone, or outcome that requires multiple independently deliverable changes.
- Keep sibling work out of a child issue. Record dependencies explicitly instead of absorbing them.
- Split security remediation from public disclosure when publishing details would increase risk.
- Do not create performative micro-issues for trivial steps that belong in one implementation.

## Draft the issue

Use `.github/ISSUE_TEMPLATE/implementation.md` as the canonical structure.

- Write an outcome-oriented title in plain language.
- Explain the user or technical consequence before implementation detail.
- Make the desired outcome observable from the product, API, repository, or development workflow.
- Name relevant projects, paths, contracts, plans, ADRs, and earlier decisions in **Context**.
- State both **Scope** and **Non-goals** so an implementer does not include adjacent cleanup.
- Write acceptance criteria as testable end states, not activities.
- Address contract, security/data, documentation, and verification expectations explicitly. If one does not apply, say why rather than inventing work.
- Put exact commands and meaningful manual flows in **Verification**.
- Put unresolved decisions in **Risks and open questions**. If a decision materially changes scope, resolve it before marking the issue ready for implementation.

Do not prescribe an implementation unless an existing decision or boundary requires it. Preserve room for the implementer to choose the smallest correct approach.

## Create issue hierarchies

For a parent issue:

- Describe the aggregate outcome and why it matters.
- List child issues in intended dependency order.
- Keep parent acceptance criteria at milestone level.
- Treat the parent as complete only when every required child outcome is complete and the integrated result is verified.

For each child issue:

- Link the parent.
- State dependencies on siblings without copying their scope.
- Include all context required to implement the child independently.
- Keep verification specific to the child.

Use GitHub subissues when available. Otherwise, use a task list of linked issues in the parent and a parent link in each child.

## Review before publishing

Confirm that:

- the issue matches checked-out code and current documentation;
- the title and opening paragraphs make sense to an external reader;
- no credentials, personal data, private URLs, or exploitable vulnerability detail is exposed;
- acceptance criteria can be conclusively checked;
- commands use versions and scripts from the repository;
- contract impact covers API status, headers, JSON, authentication, web, and Android where relevant;
- non-goals prevent unrelated refactoring;
- the issue does not duplicate or contradict an existing issue;
- labels, milestone, parent, project, and status are correct.

Draft locally unless the user authorizes GitHub writes. Before publishing, restate the target repository and issue hierarchy. After publishing, return the issue numbers and links.

## Implementation handoff

Use this short prompt after the issue is ready:

> Implement issue #<number>. Read its parent issue, linked plan, relevant AGENTS.md files, and current code. Plan first, identify contract impact, then implement and verify all acceptance criteria. Do not include sibling issues.
