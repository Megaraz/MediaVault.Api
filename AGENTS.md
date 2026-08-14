# AGENTS.md - MediaVault

## Scope and precedence

This is the root instruction file for AI-assisted work in this repository. It applies to the ASP.NET Core API, backend projects, tests, and backend documentation.

- Treat the checked-out code, project manifests, lock files, migrations, tests, and active task as the source of truth.
- Read the nearest nested `AGENTS.md` if one is added later; more specific instructions override this file for that subtree.
- Distinguish clearly between **current behavior**, **approved work in progress**, and **future direction**. Never describe roadmap items as implemented.
- Revalidate version-sensitive claims from `.csproj` files and official versioned documentation.
- Keep this file limited to durable repository-wide guidance. Put temporary status, phase plans, and detailed design decisions in `docs/`.
- Update this file when a repository-wide invariant changes. Do not preserve guidance that the code has made false.

## Product mission

MediaVault is a personal media library for movies, TV series, games, books, and manga. A user can build a library, track status and ratings, write reviews, and enrich entries with metadata from external services.

The project began as a solo final-year backend-development project and now serves three equal goals:

1. A credible public portfolio project, with readable code and honest technical documentation.
2. A live web and Android product that the developer can genuinely use.
3. A deliberate learning environment for improving professional development judgment.

Optimize for correctness, security, data integrity, maintainability, and user value. Prefer production-minded simplicity over either prototype shortcuts or enterprise ceremony. Code should be understandable to the repository owner and explainable in a technical interview.

## Repository and ecosystem boundaries

This repository owns the backend API. The React web app and Expo/React Native mobile app live in the separate `Megaraz/MediaVault.Clients` repository. The Megaraz ResultPattern libraries are separate published NuGet packages with their own repositories.

- Do not modify sibling repositories or published packages unless the user explicitly puts them in scope.
- Treat API routes, authentication, HTTP statuses, JSON shapes, error codes, pagination, and sync metadata as contracts shared by multiple clients.
- Before intentionally changing a shared contract, find its web and Android consumers. Update all in-scope consumers together or document the exact compatibility gap.
- Do not copy package source into MediaVault to make a local fix. Change an application-owned adapter/policy, or make a separately scoped change in the package repository.
- Never rely on unpublished local package changes for a solution that is meant to build from the public repository.

## Current system map

Revalidate this map before architectural work:

- `media-vault-app.Domain`: entities, value objects, enums, and domain-facing contracts.
- `media-vault-app.Application`: use cases, service contracts and implementations, DTOs, validation, and mapping.
- `media-vault-app.Infrastructure`: EF Core, SQLite persistence, repositories, migrations, and third-party HTTP clients.
- `media-vault-app.API`: composition root, JWT bearer authentication, controllers, and HTTP response mapping.
- `Rasmus.SharedKernel`: shared entity/service contracts plus MediaVault-owned validation, pagination, result-message, diagnostic, and logging abstractions. It depends only on the published core ResultPattern package.
- `media-vault-app.Tests` and `Rasmus.SharedKernel.Tests`: xUnit tests.
- `docs/`: durable plans and architecture documentation.

The backend currently targets .NET 10 and EF Core with SQLite. External metadata comes through backend integrations for RAWG, TMDB, and Google Books. Exact versions and behavior must still be verified from code.

The following are directions, not claims about current implementation: explicit timeout/retry/rate-limit policies, React Query, offline mobile synchronization, Sentry or another production telemetry backend, production deployment, and AI recommendations. The global exception boundary, vendor-neutral OpenTelemetry baseline, local Aspire AppHost orchestration in the `Megaraz/MediaVault` workspace, and the optional standalone Aspire Dashboard workflow are implemented.

## Architecture rules

- Preserve the current layered dependency direction.
- Domain must not depend on EF Core, ASP.NET Core, HTTP clients, UI code, or operational telemetry. Do not expand its transitional SharedKernel dependency without a concrete reason.
- Application owns business workflows, validation, DTO mapping, and ports. It must not acquire database or ASP.NET Core dependencies merely for convenience.
- Infrastructure owns persistence, external HTTP implementation details, and application-facing operational adapters.
- API is the composition and transport boundary. Keep controllers thin: obtain the authenticated user, validate transport concerns, call Application, and map the result.
- Put business rules in the narrowest layer that can enforce them consistently.
- Reuse an existing abstraction when it genuinely fits. Do not force special behavior through a generic base class or introduce a new abstraction until concrete duplication or a real boundary justifies it.
- Prefer explicit code over clever generic constraints, reflection, service locators, or hidden side effects.
- Do not add a new top-level project, architectural layer, state library, mediator, mapping framework, or other major pattern without comparing it to a smaller change.

## Result Pattern and exception handling

Expected failures are values; unexpected failures are exceptions.

- Use `Result`/`Result<T>` for expected outcomes such as validation failure, not found, conflict, authorization decisions made by application logic, and known upstream failures.
- Do not throw exceptions for normal control flow.
- Do not catch an exception only to hide it, return a successful response, or discard diagnostic context.
- A global exception boundary is for genuinely unhandled failures. It must log once, return a safe and consistent 5xx response, preserve cancellation semantics, and never expose stack traces, SQL, secrets, or internal exception messages.
- Keep HTTP mapping centralized. Services and repositories must not decide MVC response types.
- Keep user-facing messages safe and stable; keep technical detail in structured diagnostics.

`docs/resultpattern-migration-plan.md` is the completed migration record and source of truth for the approved compatibility decisions governing `Megaraz.ResultPattern`, `Megaraz.ResultPattern.AspNetCore`, and `Megaraz.ResultPattern.Infrastructure`.

- Preserve its D1-D11 decisions unless a later task explicitly records a superseding owner decision.
- Preserve MediaVault-owned behavior that packages do not provide, including application-specific validation helpers, logging, pagination policy, and `CreatedAtAction` behavior.
- Do not reintroduce a duplicate local ResultPattern implementation or compatibility bridge.
- Test application contracts and policies, not package internals already tested by the package projects.

## API, authentication, and security

- Authentication is currently JWT bearer authentication. Do not describe it as cookie authentication or silently replace it.
- Obtain the current user identifier from validated claims for user-owned operations. Never trust a client-supplied owner ID as authorization.
- Preserve authorization on protected controllers and endpoints. New public endpoints require an explicit reason.
- Keep token creation and attachment centralized. Do not hand-build authorization headers throughout the UI.
- Never log or commit passwords, password hashes, JWTs, signing keys, API keys, connection secrets, or sensitive personal data.
- Store local secrets in user secrets or environment variables. Bind configuration through typed options and fail fast on invalid required configuration.
- Keep CORS origins environment-specific and minimal. Do not broaden credentialed CORS to arbitrary origins.
- Password hashing, token validation, issuer/audience checks, token lifetime, and logout semantics are security-sensitive. Add focused tests when changing them.
- Return the minimum required user data. Review DTO changes for accidental exposure of password hashes, tokens, email addresses, or internal identifiers.
- Validate and bound all client input, including search text, pagination, uploaded or remote URLs, and AI prompt parameters.

Any public API change must consider status codes, headers, response body, error schema, OpenAPI output, web behavior, Android behavior, and backward compatibility.

## Data and SQLite

- EF Core migrations are the schema history. Change the model and add a reviewed migration; do not hand-edit the database as a substitute.
- Preserve the `MediaEntry` inheritance model and discriminator deliberately. Changes to TPH mapping, ownership, delete behavior, indexes, or the `Rating` value object require migration and data-compatibility review.
- Treat checked-in or local `.db`, `.db-wal`, and `.db-shm` files as potentially stateful runtime data. Do not overwrite, delete, regenerate, or stage them unless the task explicitly requires it and their role has been verified.
- Never solve Android synchronization by copying or sharing the backend SQLite file.
- Before implementing offline sync, define server authority, stable identifiers, change/version tracking, tombstones, idempotency, conflict policy, clock assumptions, migration behavior, and retry recovery. Document these decisions before building a general sync engine.
- Database exceptions must become safe application failures or reach the global unexpected-failure boundary; raw database details must not reach clients.

## Cancellation, timeouts, retries, and rate limits

These capabilities must be designed per boundary, not added as blanket middleware.

- Propagate `CancellationToken` through controller, application, repository, EF Core, and outbound HTTP calls.
- Distinguish caller cancellation from timeout and transport failure. Do not turn a cancelled request into a generic 500 or log it as an unexpected server error.
- Every external request should eventually have an explicit timeout budget. Keep the total user-request budget in mind when composing calls.
- Retry only transient failures and only when the operation is safe or explicitly idempotent. Never blindly retry writes, authentication, validation failures, most 4xx responses, or caller cancellation.
- Bound retries, add jitter where appropriate, respect `Retry-After`, and record the final outcome without logging the same failure at every layer.
- Rate-limit at a meaningful boundary and return a stable 429 contract. Consider per-user and per-IP behavior, authenticated versus anonymous endpoints, external-provider quotas, and test/dev configuration.
- Do not add a resilience package until the desired policy, ownership, observability, and tests are clear.

## Logging, diagnostics, and observability

The legacy NDJSON file logger has been removed. Do not recreate it or introduce
parallel application logging systems.

- Prefer structured events with stable names and useful fields over interpolated prose.
- Include correlation/trace identifiers and operation context where useful, but exclude secrets and unnecessary personal data.
- Avoid logging the same error in repository, service, controller, middleware, and external monitoring. Define one ownership point for each failure class.
- OpenTelemetry instruments traces, metrics, and logs through standard .NET abstractions; keep exporter/vendor configuration outside business code.
- If adopting Sentry, define what it adds beyond logs/traces, scrub sensitive data, and configure sampling and environments deliberately.
- The local Aspire AppHost/dashboard and a hosted error-monitoring service solve different problems. Do not treat them as interchangeable; document development and production responsibilities plus hosting cost before adopting a production backend.
- Observability failures must not break normal application behavior.

## External APIs and future AI features

- Keep third-party credentials and model API keys on the backend.
- Follow the existing external-integration boundary: typed options, configured `HttpClient`, Infrastructure client, Application service, API endpoint, then client UI.
- Treat upstream payloads and error text as untrusted. Validate deserialization, size limits, nullability, attribution/licensing requirements, and safe user messages.
- Preserve provider-specific concerns at the edge; expose MediaVault-owned DTOs to clients.

For AI recommendations:

- Keep the first version narrow, optional, and non-critical to core media-library use.
- Send only the minimum data needed for the recommendation. Do not send reviews, account data, tokens, or a full private library by default.
- Make media type and other user constraints explicit, validate model output, and handle refusal, timeout, malformed output, rate limits, and provider outage.
- Define cost limits, request limits, telemetry, data-retention expectations, and a non-AI fallback before public deployment.
- Treat prompts, model identifiers, and output schemas as versioned application behavior with tests where practical.
- Never let generated text directly perform privileged actions or write trusted data without validation and user confirmation.

## Working method

1. Read the relevant code, tests, manifests, and active design document before proposing a change.
2. Check `git status` and preserve unrelated user changes. Never restore, delete, or reformat them as cleanup.
3. Establish the narrowest useful baseline. Record pre-existing failures instead of claiming the change caused or fixed them.
4. For substantial work, state the intended behavior, affected boundaries, contract impact, risks, and verification before implementation.
5. Make the smallest coherent change. Avoid opportunistic rewrites and formatting churn.
6. Add or update focused tests with the behavior.
7. Update documentation when setup, architecture, configuration, migration status, public behavior, or an important tradeoff changes.
8. Run the narrowest relevant verification, then broaden it in proportion to risk.
9. Report what changed, what was verified, any remaining warnings/failures, and any manual step without overstating completion.

Do not add or upgrade a production dependency without checking official documentation and explaining why the existing platform or dependencies are insufficient. Do not commit, push, open a pull request, publish, deploy, mutate external services, or change sibling repositories unless the user asked for that action.

## Validation commands

Use exact versions and scripts from the checked-out manifests. Common commands from the repository root are:

```powershell
dotnet test media-vault-app.slnx
```

- Prefer a focused test project or test filter while iterating, then run the broader command when the risk warrants it.
- A command that fails before the change is baseline evidence, not permission to ignore it. Keep new work from adding failures and report unrelated existing failures precisely.
- For database changes, inspect the generated migration and test upgrade behavior against representative data.
- For API changes, add contract-focused tests for authorization, status, headers, error bodies, cancellation, and cross-user isolation as applicable.

## Code review rules

Prioritize findings that can cause:

- cross-user data access or missing authorization;
- secret, token, password, or personal-data exposure;
- data loss, destructive migration behavior, or sync conflicts;
- breaking HTTP/error/auth contracts for either client;
- expected failures thrown as exceptions, or unexpected failures hidden as successful results;
- swallowed cancellation or indiscriminate retry of non-idempotent work;
- unsafe upstream or AI content reaching users or trusted storage;
- dependencies pointing at unpublished local paths;
- architectural dependency inversion;
- tests that pass while asserting the wrong contract.

Do not spend review attention on formatting that tooling can enforce unless it obscures correctness.

## Definition of done

A task is done only when:

- the requested behavior is implemented at the correct boundary;
- security, user ownership, data integrity, cancellation, and compatibility were considered;
- relevant tests and builds pass, or exact pre-existing/blocking failures are reported;
- public contracts and all in-scope consumers agree;
- configuration contains no secrets and has a documented setup path;
- migrations and operational behavior are reviewed when relevant;
- documentation and active plan status match reality;
- the diff contains no unrelated changes or accidental generated/runtime files.

## Collaboration and learning

The repository owner wants both a working result and stronger engineering judgment.

- Be direct, constructive, and specific.
- Explain important framework conventions, tradeoffs, and failure modes without turning routine edits into lectures.
- For unfamiliar areas such as exception handling, resilience, sync, observability, deployment, or AI, make the policy and reasoning visible before hiding it behind a library.
- Complete requested implementation unless the user asks for guided-only work; learning value is not a reason to leave a safe, in-scope task half-finished.
- Avoid inflated claims such as "enterprise-grade." Prefer evidence: tests, documented tradeoffs, reproducible setup, observable behavior, and a working deployment.
