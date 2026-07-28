# Contributing to MediaVault

MediaVault is an active pre-release product, public portfolio, and learning
project maintained by one developer. Focused contributions are welcome after
prior discussion, but opening an issue or pull request does not guarantee that
the proposed work will be accepted.

## Before starting

1. Search the issue tracker and the
   [MediaVault product project](https://github.com/users/Megaraz/projects/2).
2. Open or comment on an issue before implementing a change. Agree on the
   observable outcome, scope, non-goals, acceptance criteria, and verification.
3. Wait for the issue to be accepted for implementation and confirm its base
   branch. Do not absorb work owned by a parent, sibling, or roadmap issue.
4. Read `AGENTS.md` and any more specific instructions near the files in scope.

Security vulnerabilities must follow [SECURITY.md](SECURITY.md), not the normal
issue workflow. Participation is governed by the
[Code of Conduct](CODE_OF_CONDUCT.md).

## Make a focused change

- Create a short-lived branch for one issue and keep commits intentional.
- Preserve the layered architecture and keep application behavior at the
  narrowest boundary that can enforce it consistently.
- Treat API routes, authentication, status codes, JSON and error shapes,
  persistence identifiers, pagination, and synchronization metadata as
  contracts shared by the web and Android clients.
- Update every in-scope consumer when a shared contract intentionally changes.
  Otherwise, preserve the contract and document any out-of-scope compatibility
  gap.
- Keep credentials, personal data, local databases, environment files, build
  output, logs, and editor state out of commits.
- Add focused tests for behavior changes and update documentation when setup,
  configuration, contracts, architecture, or user-visible behavior changes.

## Verify the work

Run the narrowest checks while iterating, then the relevant repository checks
before requesting review:

```powershell
dotnet test media-vault-app.slnx

Push-Location media-vault-app.client
npm ci
npm run lint
npm run build
Pop-Location

git diff --check
```

If an unrelated, pre-existing failure blocks a check, record the exact command
and failure in the pull request and link its tracking issue. Do not hide the
failure or weaken verification to make the change appear green.

## Open the pull request

Keep the pull request reviewable and include:

- the issue outcome and why it matters;
- important implementation or policy decisions;
- API, authentication, security, data, and client-contract impact;
- exact verification commands and results;
- remaining risks or follow-up issues; and
- a closing reference such as `Closes #123` when the pull request should close
  an issue on merge.

Do not include unrelated cleanup, generated/runtime files, or sibling-issue
work. Maintainers may request changes or decline work that does not fit the
product direction, even when the implementation is technically sound.
