---
name: write-github-issues
description: Create, refine, or review professional GitHub issues and parent/subissue hierarchies for MediaVault. Use when Codex drafts or publishes implementation issues, turns plans or backlog items into AI-executable tasks, prepares sprint or milestone issues, or improves issue specifications for technically rigorous public work.
---

# Write GitHub Issues

Create issues that communicate the technical problem clearly to maintainers and contributors and are executable by an AI agent without hidden context.

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

## Write safely for public Projects and issues

- Treat the repositories, GitHub Projects, issues, pull requests, and comments as public and permanently discoverable unless current GitHub state proves otherwise.
- Lead with the technical or user problem, its consequence, and the observable engineering outcome. Do not justify routine work through recruiter appeal, portfolio optics, contribution counts, AI activity, or performative professionalism.
- Mention public presentation, hiring, learning, or build-in-public communication only when that is the issue's actual product or documentation outcome. Even then, keep technical claims evidence-based and secondary motivations concise.
- Do not publish secrets, personal data, private URLs, internal incident detail, exploit steps, vulnerable payloads, embargoed findings, private advisory content, or security evidence that would materially help an attacker.
- In a public security issue, describe the affected control or boundary and the safe end state. Keep detailed alert inventories, vulnerable-version analysis, reproductions, logs, and exploitability decisions in GitHub's private security features or another approved private channel.
- Do not put exact private alert counts, severity distributions, or signed-in-only security-dashboard output into a public issue unless the owner explicitly approves that disclosure and it is necessary for the work.
- Split public remediation tracking from confidential disclosure when one issue cannot be both executable and safe. The public issue may require private evidence without copying that evidence into its body or comments.
- Use repository-relative paths, public documentation, stable symbols, and safe verification commands as context. Avoid machine-local paths and hidden context that a future implementer cannot access.

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

- current repository and Project visibility has been checked, and the issue is safe to index publicly;
- the issue matches checked-out code and current documentation;
- the title and opening paragraphs make sense to an external reader;
- no credentials, personal data, private URLs, signed-in-only security evidence, or exploitable vulnerability detail is exposed;
- the problem and desired outcome are framed around technical or user value rather than recruiter, portfolio, or activity optics;
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
