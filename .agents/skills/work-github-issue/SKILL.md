---
name: work-github-issue
description: Take a MediaVault GitHub implementation issue through a focused branch, implementation, verification, commit, push, Project status transition, and review-ready pull request. Use when the user asks Codex to work, implement, complete, or take an issue through PR/review. Do not use for issue authoring alone, parent-roadmap planning, PR review feedback, CI-only debugging, or merging.
---

# Work GitHub Issue

Take one ready implementation issue from intake to a review-ready pull request. Preserve the owner’s merge gate.

## Resolve the task

1. Read the nearest applicable `AGENTS.md`.
2. Resolve the repository and issue from a full URL/reference or the current checkout. If an issue number is ambiguous across MediaVault repositories, stop and request the exact `OWNER/REPO#NUMBER`.
3. Read the issue, its formal parent, linked plan/ADR, dependencies, and relevant sibling titles. Do not absorb sibling scope.
4. Inspect the current code, tests, manifests, and documentation named by the issue.
5. Confirm the issue has an observable outcome, bounded scope/non-goals, testable acceptance criteria, verification, and no unresolved decision that materially changes implementation.
6. Search for an existing branch or pull request for the issue. Resume coherent existing work rather than creating duplicates.

If the issue is not implementation-ready, report the exact gap and refine it only when the user has authorized issue editing. Do not code around a missing product, contract, security, or data decision.

## Prepare isolated work

1. Check `git status` and preserve unrelated user changes.
2. Identify the intended base branch from the repository and existing work; do not assume `main`.
3. Create or reuse one branch for the issue:

   ```text
   codex/issue-<number>-<short-kebab-slug>
   ```

4. In a Codex-managed worktree, create the branch before committing. Remember that Git permits a branch to be checked out in only one worktree at a time.
5. Target only `@Megaraz's MediaVault app` Project 2. Never add or move work in the old final-year Project.
6. Move the implementation issue—not its roadmap parent—to **In progress** only after the task is ready and its branch exists.

Do not rewrite history, force-push, discard changes, modify sibling repositories, or add production dependencies unless the issue and user authorization put that action in scope.

## Plan and implement

Before editing, state:

- intended observable behavior;
- affected architecture boundaries;
- API/auth/data and web/Android contract impact;
- security, privacy, cancellation, and compatibility concerns;
- planned verification;
- important risks or decisions.

Implement the smallest coherent change that satisfies the issue. Follow its non-goals. Avoid opportunistic refactors, formatting churn, performative commits, and work owned by sibling issues.

Add or update focused tests with behavior changes. Update documentation when setup, configuration, contracts, architecture, migrations, public behavior, or operational expectations change.

## Verify and self-review

1. Check every acceptance criterion individually.
2. Run the issue’s narrow verification first, then broaden checks in proportion to risk.
3. Record exact pre-existing or blocking failures; do not claim they passed.
4. Review the final diff for:
   - unrelated changes or generated/runtime files;
   - secrets, personal data, database files, or machine-local paths;
   - contract mismatches across API, web, and Android;
   - missing authorization or cross-user isolation;
   - data-loss, migration, cancellation, retry, or error-mapping regressions;
   - documentation that presents planned behavior as implemented.
5. Run `git diff --check` and confirm only issue-owned files will be committed.

Do not publish a review-ready PR while an acceptance criterion is unverified, a relevant check fails because of the change, or a material decision remains unresolved. Report the blocker and leave an honest Project status.

## Commit, push, and open the PR

Invocation through PR authorizes the normal issue-owned branch, Project, commit, push, and PR actions. It does not authorize merge, deployment, visibility changes, destructive cleanup, or unrelated external actions.

1. Stage only files belonging to the issue.
2. Create an intentional commit with a concise outcome-oriented message.
3. Push the issue branch without force.
4. Open one **ready-for-review** pull request against the verified base branch. Use a draft only when the user explicitly requests early/incomplete review.
5. Include:
   - issue outcome and user/technical value;
   - important implementation decisions;
   - contract/security/data impact;
   - verification commands and results;
   - remaining risks or follow-up issues;
   - `Closes #<number>` for a same-repository issue, or the full closing reference when needed.
6. Verify the PR URL, base/head branches, linked issue, and checks.
7. Move the issue to **In review** only after the pushed branch and PR both exist.

Never merge the PR, mark the issue Done, close it manually, or delete branches as part of this skill. The owner reviews and merges. The closing keyword should close the issue on merge; a separate post-merge task may verify the result and move it to Done.

## Report the handoff

Return:

- issue, branch, commit, and PR links/identifiers;
- concise implementation summary;
- verification performed and exact results;
- Project status;
- known warnings, blockers, or follow-ups;
- explicit statement that the PR was not merged.

For later review comments, use the GitHub review-feedback workflow on the existing branch. For failing Actions checks, use the GitHub CI-fix workflow. Do not create a replacement branch or PR.
