# AGENTS.md - MediaVault

## Purpose

This file is the root instruction set for AI work in this repository. Treat the current codebase as the source of truth. If you suggest a target direction that does not exist yet, label it clearly as desired future state rather than current implementation.

Use this file as the single root instruction file for the repo. Do not duplicate these instructions in `copilot-instructions.md` or other tool-specific files. If a tool-specific file is required, make it a short pointer to this `AGENTS.md`.

## Source-of-truth rules

- Treat the codebase as more authoritative than this file when they disagree.
- When version-specific guidance matters, verify current versions from `.csproj`, `package.json`, `Directory.Packages.props`, lock files, or other project files.
- Do not create new top-level folders, projects, architectural layers, or naming schemes without first checking whether an existing convention already fits.

## Project snapshot

MediaVault is a full-stack media logging application for movies, TV series, games, books, and manga.

The project started as a school project, but it is now being polished into:

1. A portfolio-quality public repository.
2. A maintainable personal web app.
3. A deliberate learning project for professional-level software practices.
4. A home for a reusable SharedKernel that should stay useful outside this app.

Optimize for maintainability, readability, testability, and learning value. Avoid enterprise-style complexity unless it solves a real problem already present in the repo.

## Strategic priorities

Treat these as the current default priority order unless the active task clearly justifies a different sequence.

1. Finish and harden SharedKernel tests.
2. Document SharedKernel public behavior.
3. Add missing backend tests for MediaVault.
4. Improve frontend structure and error handling.
5. Add documentation for setup, architecture, database, APIs, and testing.
6. Improve logging and observability.
7. Add caching where it solves a real problem.
8. Prepare deployment.
9. Polish README and portfolio presentation.
10. Publish a live demo when the repo is stable enough.

## AI working rules

- First understand the existing code before changing it.
- Prefer small, reviewable changes.
- Do not rewrite large areas unless explicitly asked.
- Explain architectural tradeoffs when they matter to the change.
- Preserve the developer's learning process. Do not optimize for speed at the cost of clarity.
- Avoid dumping huge finished solutions when guidance, staged implementation, or smaller commits would teach more.
- Match the current project style and level of abstraction.
- When uncertainty remains, say so explicitly and suggest the safest next step.
- Do not add new packages without explaining why the existing stack is not enough.
- Do not introduce major patterns without comparing them to simpler alternatives.
- Do not delete files, remove public APIs, rename projects, or perform broad formatting-only changes unless explicitly asked or clearly required by the current task.
- Prefer code that a junior developer can understand, debug, and maintain.
- Aim to improve the repo as both a public portfolio project and a real personal app.

## Change size guidance

For larger changes, prefer this order:

1. Explain the intended change.
2. Identify affected projects and files.
3. Make the smallest useful implementation.
4. Add or update tests.
5. Update documentation if behavior, setup, or public API changed.

## Memory and lessons learned

- For larger repo-specific work, consult the current AI context and lessons before proposing broad changes.
- Keep user-visible durable lessons in `.github/docs/ai/LESSONS_LEARNED.md`.
- Keep concise AI-facing durable lessons and recurring gotchas in `memories/repo/lessons_learned.md` relative to the repository root.
- Update lessons only when they are verified by the codebase, tests, or repeated experience.
- Do not dump temporary task history, guesses, or stale contradictions into lessons-learned files.

## Current repo map

- `media-vault-app.Domain`: entities, enums, value objects, and domain-facing interfaces.
- `media-vault-app.Application`: DTOs, validators, mappers, service interfaces and implementations, and shared service base classes.
- `media-vault-app.Infrastructure`: `AppDbContext`, EF Core mappings, repositories, migrations, and external API clients.
- `media-vault-app.API`: controllers, auth setup, DI composition root, and HTTP/result mapping.
- `media-vault-app.client`: React 19 + React Router 7 + TypeScript + Vite + Tailwind CSS v4 frontend.
- `Rasmus.SharedKernel`: Result Pattern, reusable abstractions, validator helpers, and error logging contracts.
- `media-vault-app.Tests` and `Rasmus.SharedKernel.Tests`: xUnit test projects.

## Architecture rules

- Respect the current Onion/layered split. Domain must stay free of EF Core, HTTP, and UI concerns.
- Keep business logic out of controllers. Controllers should handle auth/user context, call services, and map `Result` objects to HTTP responses.
- Application owns business workflows, DTO validation, mapping, and service coordination.
- Infrastructure owns EF Core, persistence, migrations, and third-party HTTP details.
- The backend already uses read/write service separation plus shared base service classes. Reuse those patterns when they help, but do not add more abstractions unless they remove concrete duplication.
- SharedKernel should remain MediaVault-agnostic. If you change public SharedKernel behavior, keep the API boring, reusable, and covered by tests in `Rasmus.SharedKernel.Tests`.

## Backend conventions

- The current backend stack is .NET 10, ASP.NET Core Web API, EF Core 10, SQL Server, and the custom Result Pattern from SharedKernel.
- Use `Result` or `Result<T>` for expected application outcomes. Do not use exceptions for normal control flow.
- Keep HTTP mapping centralized through the existing result-mapping layer instead of scattering `BadRequest`, `NotFound`, `Conflict`, and similar decisions through services.
- Validation currently uses custom validator classes in Application, not FluentValidation. Do not introduce a second validation style casually.
- Pass `CancellationToken` through async controller, service, repository, and client flows. The codebase already does this consistently.
- Authentication is cookie-based, not JWT-based. Do not replace or bypass that without an explicit architectural decision.
- Match the existing backend naming and namespace style, including the current `media_vault_app.*` root namespaces.
- When adding configuration, follow the existing `IOptions<T>` + data annotations + `ValidateOnStart()` pattern.
- External integrations currently go through the backend for RAWG, TMDB, and Google Books. Follow the same structure: options class, typed HttpClient, application service, API controller, then frontend client if the UI needs it.
- Error logging uses the SharedKernel logger and writes NDJSON files at runtime. Extend that flow instead of inventing a parallel logging approach.

## Domain and data model notes

- `MediaEntry` is an inheritance hierarchy with concrete types for movie, TV series, game, book, and manga.
- EF Core uses TPH with a `MediaType` discriminator. Preserve that unless there is a strong, migration-backed reason to change it.
- `Rating` is a value object with database precision and check constraints.
- TV series and seasons use owner/dependent relationships and shared base abstractions. Keep that pattern coherent when touching series or seasons.
- When adding media features, preserve the existing type-specific DTO, mapper, controller route, and frontend client structure instead of collapsing everything into one loose payload.

## Frontend conventions

- The current frontend stack is React 19, React Router 7, TypeScript 5.9, Vite 7, and Tailwind CSS v4.
- The frontend is functional but still early-stage. Treat it as an MVP that should be steadily cleaned up, not as a finished design system.
- State management currently uses React Context (`UserContext`) plus local component state. Do not introduce Redux, Zustand, React Query, or similar libraries unless the change clearly justifies it.
- API calls live in `src/Clients`. Prefer extending that layer instead of embedding fetch calls directly in UI components.
- Auth and API traffic currently rely on relative URLs and the Vite HTTPS proxy. Do not hardcode dev API base URLs.
- Cookie-based auth flows depend on sending credentials. Preserve the current authenticated request pattern when changing frontend clients.
- Tailwind is configured through `@tailwindcss/vite` and `@import "tailwindcss"` in `src/index.css`. There is no `tailwind.config.js` in the current setup.
- Some frontend files are transitional or prototype-quality. Improve them when you touch them, but do not copy rough patterns forward into new work.

## Build, config, and validation

- The solution file is `media-vault-app.slnx`.
- The API project is configured to work with the Vite dev server through SpaProxy and the Vite HTTPS proxy.
- Connection strings and external API credentials are intentionally not committed in `appsettings.json` or `appsettings.Development.json`. Use user secrets or environment variables.
- Backend changes should usually be validated with the narrowest relevant `dotnet test` invocation. Broad fallback: `dotnet test media-vault-app.slnx`.
- Frontend changes should usually be validated with `npm run build` or `npm run lint` in `media-vault-app.client`, depending on the change.

## Testing expectations

- Existing automated coverage is strongest around validators, SharedKernel Result behavior, pagination helpers, and one external API service. There are currently no real integration tests and no frontend test suite.
- Do not describe missing tests as if they already exist.
- When you add behavior, prefer adding focused tests near the touched code.
- Prefer xUnit tests with clear names in the `Method_WhenCondition_ShouldExpectedOutcome` style.
- Existing tests often use simple manual fakes instead of heavy mocking libraries. Prefer the simplest test double that keeps the test readable.

## Review checklist

- Before considering a task complete, check that the relevant code compiles.
- Check that relevant tests pass, or state clearly when they could not be run.
- Check that the change still fits the existing architecture and layering.
- Check that naming is clear and intent-revealing.
- Check that no unnecessary abstraction or indirection was introduced.
- Check that errors still flow through the existing Result and logging approach where applicable.
- Check whether setup, behavior, or public API changes require documentation updates.
- Check that API keys, tokens, and secrets remain out of source control.
- Check whether the change would still make sense in a public portfolio repository.
- If validation commands cannot be run, state exactly what was changed and which command should be run manually.

## Documentation standards

- Documentation is part of the product, not cleanup work for later.
- Documentation should be practical, honest, and recruiter-friendly.
- Important docs to maintain over time include the root README, setup instructions, architecture overview, database overview, external API documentation, SharedKernel documentation, testing guide, deployment guide, and ADRs for major architectural decisions.
- Some of those documents are still missing or incomplete in the current repo. Treat them as active documentation debt, not as already-solved work.
- When writing docs, explain why choices were made and what tradeoffs were accepted.
- Include screenshots or GIFs when they materially improve understanding, especially in the public-facing README.
- Mention honestly that the project began as a solo school project and was later polished into a stronger portfolio and personal-use codebase.
- Avoid inflated wording such as "enterprise-grade" unless the repository genuinely supports that claim. Prefer honest language such as "production-inspired", "portfolio-quality", or "business-level direction".

## Future AI customization

- Keep this file short and repo-wide. Put deeper or task-specific AI guidance under `.github/docs/ai`, `.github/prompts`, `.github/agents`, or `.github/skills` as those are added.
- Future AI prompts, agents, and skills should match the actual current codebase first and separate current state from desired direction explicitly.
- If you introduce a new long-lived repo convention, update this file so later AI customizations inherit the same baseline.

## Communication style

- The developer is still learning and wants feedback that improves judgment, not just output.
- Be direct but constructive.
- Explain why, not only what.
- Prefer practical examples over abstract preaching.
- Point out risks and tradeoffs clearly.
- Avoid unnecessary jargon.
- Do not overpraise weak code.
- Do not be needlessly harsh.
- Treat the project as serious work, while staying realistic about what fits a solo junior developer portfolio project.