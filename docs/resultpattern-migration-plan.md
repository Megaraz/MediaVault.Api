# MediaVault ResultPattern Migration

## Document status

- **Purpose:** Source of truth for replacing MediaVault's internal ResultPattern implementation with the published Megaraz NuGet packages.
- **Scope:** Backend only.
- **Status:** Core package type identity is migrated; the database, outbound HTTP, ASP.NET mapping, logger relocation, and legacy-removal phases remain.
- **Initial analysis:** 2026-07-26.
- **Document created:** 2026-07-28.
- **Rule:** Update this document when a compatibility decision is made, a phase is completed, or implementation discovers a material difference from this baseline.
- **Non-goal:** Do not combine this migration with unrelated architecture changes, refactoring, or cleanup.

## Target packages

The migration targets these published package versions:

| Package | Version | Source repository |
|---|---:|---|
| `Megaraz.ResultPattern` | `0.2.2` | https://github.com/Megaraz/Megaraz.ResultPattern |
| `Megaraz.ResultPattern.AspNetCore` | `0.1.1` | https://github.com/Megaraz/Megaraz.ResultPattern.AspNetCore |
| `Megaraz.ResultPattern.Infrastructure` | `0.1.0` | https://github.com/Megaraz/Megaraz.ResultPattern.Infrastructure |

All three packages support `net10.0`. The coexistence baseline restores these exact versions from `https://api.nuget.org/v3/index.json`; no repository or machine-local package source is required.

## Executive summary

The migration is feasible and can remain buildable throughout, but it is not a namespace-only replacement.

The core `Result` APIs are close to the local implementation. The material compatibility differences are:

- Package error factories do not populate `UserMessage` unless the caller explicitly supplies it.
- Package validation failures default `Result.Message` to an empty string.
- Package `ErrorContext` removes layer, service, and method metadata.
- Database and HTTP errors are both classified as `ErrorType.External`.
- Package database error codes use `Database...` rather than the local `Db...` reason names.
- Package HTTP mapping treats non-HTTP `External` errors as HTTP 502 by default, while MediaVault currently maps database errors to 500.
- The package validation response body adds a top-level `code`.
- Outbound HTTP response mapping applies a 64 KiB body limit by default.
- The packages do not provide MediaVault's NDJSON logger, logging policy, generic ID/null validation helpers, or the exact `CreatedAtAction` adapter.
- The package pagination helper lives in the ASP.NET Core package and requires an explicit maximum page size, while MediaVault uses pagination in Application.

These behaviors must be handled deliberately before mechanical replacements begin.

## Current project dependency shape

```text
API
 ├─ Application
 ├─ Domain
 └─ Infrastructure
      ├─ Application
      ├─ Domain
      └─ SharedKernel

Application → Domain → SharedKernel
Tests → Application / Domain / SharedKernel
```

`Domain` has no direct ResultPattern callsites, but it references SharedKernel for entity contracts. Result types are exposed by SharedKernel service and repository interfaces and therefore flow transitively through the backend.

## Current implementation inventory

### Callsite footprint

The initial inventory found approximately:

- 38 production files with explicit `Result` API usage.
- 17 production files using `ErrorContext`.
- 14 production files using `ValidationError`.
- 4 repositories using `DatabaseError`.
- 3 outbound HTTP clients using `HttpError`.
- 6 Application services using `PaginationParameters`.
- 34 controller calls through `ToActionResult`, `ToNoContentResult`, or `ToCreatedResult`.
- 18 `Result.Map` calls.
- 7 failure-type conversions using `.From<TIn,TOut>()`.
- 7 outbound `MapToResultAsync` calls.
- 63 `DefineErrorContext` calls or declarations.
- 40 directly affected test files: 25 SharedKernel tests and 15 application tests.

### Locally implemented types

| Area | Local symbols | Main consumers |
|---|---|---|
| Core result | `Result`, `Result<T>`, `ResultExtensions` | SharedKernel contracts, Application services, repositories, HTTP clients, controllers |
| Core error | `Error`, `ErrorType`, `ErrorContext`, `OperationType` | Services, repositories, HTTP clients, logging, HTTP mapping |
| Error codes | `ErrorCode`, `ErrorReasonCode`, `ErrorReasonCodeExtensions` | Error factories and SharedKernel tests |
| Validation | `ValidationError`, `ValidationErrorType`, `Validator`, `ValidatorExtensions` | DTO validators, Application base services, external API services |
| Database | `DatabaseError`, `DatabaseErrorType` | `RepoBase`, `DependentEntityRepoBase`, `UserRepo`, `MediaEntryRepo` |
| Outbound HTTP | `HttpError`, `HttpErrorType`, `HttpResponseToResultExtensions` | Google Books, RAWG, and TMDB clients |
| HTTP responses | `HttpResultMapper`, `MappedHttpResponse`, `ErrorResponseBody`, `ValidationErrorItem`, `ValidationErrorResponseBody` | API `ResultResponseMapper`, all controllers indirectly |
| Pagination | `PaginationParameters` | Read services and external search services in Application |
| File logging | `ErrorLoggerConfiguration`, `ErrorLog`, `ErrorLogger`, `ErrorLogPolicy` | API DI, repository bases, outbound clients, cleanup hosted service |
| Logging contracts | `IErrorLogger`, `IErrorLogPolicy` | Application cleanup service, Infrastructure repositories/clients, API composition |

### Important local files

- `Rasmus.SharedKernel/ResultPattern/Result.cs`
- `Rasmus.SharedKernel/ResultPattern/Error.cs`
- `Rasmus.SharedKernel/ResultPattern/ErrorContext.cs`
- `Rasmus.SharedKernel/ResultPattern/ErrorCode.cs`
- `Rasmus.SharedKernel/ResultPattern/ValidationError.cs`
- `Rasmus.SharedKernel/ResultPattern/Validator.cs`
- `Rasmus.SharedKernel/ResultPattern/ValidatorExtensions.cs`
- `Rasmus.SharedKernel/ResultPattern/DatabaseError.cs`
- `Rasmus.SharedKernel/ResultPattern/HttpError.cs`
- `Rasmus.SharedKernel/ResultPattern/HttpResponseToResultExtensions.cs`
- `Rasmus.SharedKernel/ResultPattern/HttpResultMapper.cs`
- `Rasmus.SharedKernel/ResultPattern/MappedHttpResponse.cs`
- `Rasmus.SharedKernel/Pagination/PaginationParameters.cs`
- `Rasmus.SharedKernel/ResultPattern/ErrorLogger.cs`
- `Rasmus.SharedKernel/ResultPattern/ErrorLogPolicy.cs`
- `Rasmus.SharedKernel/Interfaces/ErrorLogger/IErrorLogger.cs`
- `Rasmus.SharedKernel/Interfaces/ErrorLogger/IErrorLogPolicy.cs`
- `media-vault-app.API/Controllers/ResultResponseMapper.cs`
- `media-vault-app.Infrastructure/Repos/RepoBase.cs`
- `media-vault-app.Infrastructure/Repos/DependentEntityRepoBase.cs`
- `media-vault-app.Infrastructure/API/Clients/ApiClientBase.cs`
- `media-vault-app.API/Program.cs`

## Project and layer dependencies

### Rasmus.SharedKernel

- Thirteen service, repository, validator, and logging contracts expose ResultPattern types.
- Contains the entire old ResultPattern implementation.
- Must eventually depend only on `Megaraz.ResultPattern`, not the Infrastructure or ASP.NET Core packages.

Affected contracts include:

- `ICreateService`
- `IDeleteService`
- `IGetByIdService`
- `IGetCollectionService`
- `IUpdateService`
- `IDependentEntityReadService`
- `IDependentEntityWriteService`
- `ISearchService`
- `IRepo`
- `IDependentEntityRepo`
- `IDtoValidator`
- `IErrorLogger`
- `IErrorLogPolicy`

### Application

- Eleven local interfaces directly expose `Result`, `ErrorContext`, or `ValidationError`.
- Four generic service bases depend on validation, mapping, failure propagation, and pagination.
- `AuthService`, `MediaEntryReadService`, and the three external API services inspect and propagate failures.
- DTO validators depend on local null, required, and matching helpers.
- `ErrorLogCleanupService` depends on the local logger contract.

Directly affected production files:

- `Interfaces/Clients/IGoogleBooksApiClient.cs`
- `Interfaces/Clients/IRawgApiClient.cs`
- `Interfaces/Clients/ITmdbApiClient.cs`
- `Interfaces/Repos/IMediaEntryRepo.cs`
- `Interfaces/Repos/IUserRepo.cs`
- `Interfaces/Services/IAuthService.cs`
- `Interfaces/Services/IGoogleBooksApiService.cs`
- `Interfaces/Services/IMediaEntryReadService.cs`
- `Interfaces/Services/IRawgApiService.cs`
- `Interfaces/Services/ITmdbApiService.cs`
- `Interfaces/Validators/IUserDtoValidator.cs`
- `Services/API/GoogleBooksApiService.cs`
- `Services/API/RawgApiService.cs`
- `Services/API/TmdbApiService.cs`
- `Services/Auth/AuthService.cs`
- `Services/Base Classes/DependentEntityReadServiceBase.cs`
- `Services/Base Classes/DependentEntityWriteServiceBase.cs`
- `Services/Base Classes/ReadServiceBase.cs`
- `Services/Base Classes/WriteServiceBase.cs`
- `Services/ErrorLogCleanupService.cs`
- `Services/MediaEntry/MediaEntryReadService.cs`
- `Services/ServiceValidationLogging.cs`
- `Validators/MediaEntry/MediaEntryDtoValidator.cs`
- `Validators/User/UserDtoValidator.cs`

### Infrastructure

- Repository exception handling creates database errors and writes NDJSON logs.
- HTTP clients convert upstream responses and transport exceptions into results.
- This is the appropriate project to reference both extension packages.

Affected files:

- `API/Clients/ApiClientBase.cs`
- `API/Clients/GoogleBooksApiClient.cs`
- `API/Clients/RawgApiClient.cs`
- `API/Clients/TmdbApiClient.cs`
- `Repos/DependentEntityRepoBase.cs`
- `Repos/MediaEntryRepo.cs`
- `Repos/RepoBase.cs`
- `Repos/UserRepo.cs`

### API

- All six controllers depend indirectly on the local response adapter.
- `ResultResponseMapper` owns `CreatedAtAction` behavior not directly provided by the package.
- `Program` registers the local logger, policy, and cleanup service.

Direct or indirect controller consumers:

- `AuthController`
- `GoogleBooksApiController`
- `MediaEntriesController`
- `RawgApiController`
- `TmdbApiController`
- `UsersController`

### Domain

- No direct ResultPattern type usage.
- Continues to depend on SharedKernel entity and identifier contracts.
- Should not receive a direct ASP.NET Core or Infrastructure package reference.

### Tests

- Most tests under `Rasmus.SharedKernel.Tests/Result Pattern` test implementation details that will become package-owned.
- Application tests construct local `ErrorContext` values and compare concrete errors, messages, descriptions, and enum categories.
- Existing HTTP mapper tests encode the current public response contract and should be treated as characterization tests until compatibility decisions are finalized.

## Local-to-package mapping

| Local API | Package equivalent | Migration classification |
|---|---|---|
| `Rasmus.SharedKernel.ResultPattern.Result` | `Megaraz.ResultPattern.Result` | Mechanical API replacement with message behavior differences |
| `Result<T>` | `Megaraz.ResultPattern.Result<T> where T : notnull` | Requires generic-constraint review |
| `ResultExtensions.Map` | `Megaraz.ResultPattern.ResultExtensions.Map` | Mostly mechanical |
| `ResultExtensions.From<TIn,TOut>` | `result.ToResult<TOut>()` or `Result<TOut>.FromResult(result)` | Mechanical rename; valid only for failures |
| `Error`, `ErrorType` | `Megaraz.ResultPattern.Error`, `ErrorType` | Enum and constructor changes |
| `ErrorContext`, `OperationType` | Core package equivalents | Constructor and metadata changes |
| `ErrorCode` | Core package `ErrorCode` | Shape changed; package exposes only `Code` |
| `ErrorReasonCode` | Obsolete package compatibility enum | Replace with `ErrorCodeReasons` or caller-owned strings |
| `ValidationError`, `ValidationErrorType` | Core package equivalents | Factories exist, but message defaults differ |
| `RequiredFieldsAreNullOrWhiteSpace` | Same compatibility method exists | Source-compatible, but creates empty `UserMessage` by default |
| `IsNullOrWhiteSpace`, `DoesNotMatch` | Same compatibility methods exist | Source-compatible, but message behavior differs |
| `Validator.IsValidId`, `IsNotValidId` | No package equivalent | Retain as MediaVault helper |
| Generic `IsNull` | No package equivalent | Retain or replace locally |
| `IsTooLow` | No package equivalent | No production usage; remove or retain only for compatibility |
| `DatabaseError` | `Megaraz.ResultPattern.Infrastructure.DatabaseError` | Factory-compatible, behavior and code changes |
| `DatabaseErrorType` | Infrastructure package equivalent | Enum values align |
| `HttpError` | `Megaraz.ResultPattern.AspNetCore.HttpError` | Factory-compatible, top-level type changes |
| `HttpErrorType` | ASP.NET Core package equivalent | Enum values align |
| `MapToResultAsync` | ASP.NET Core package equivalent | Mostly compatible; adds body limits and safe-message policy |
| `HttpResultMapper` | ASP.NET Core package equivalent | Requires a custom MediaVault policy |
| HTTP response records | ASP.NET Core package equivalents | Validation response shape differs |
| `ResultResponseMapper` | `AspNetCoreResultExtensions` | Keep a thin adapter for `CreatedAtAction` and centralized policy |
| `PaginationParameters` | ASP.NET Core package equivalent | Not recommended in Application because of layering |
| Logger and policy types | No package equivalent | Retain as MediaVault-owned logging |
| `IErrorLogger`, `IErrorLogPolicy` | No package equivalent | Retain |

## Detailed compatibility differences

### Namespaces and package placement

- Core: `Megaraz.ResultPattern`
- HTTP and MVC mapping: `Megaraz.ResultPattern.AspNetCore`
- Database errors: `Megaraz.ResultPattern.Infrastructure`

Recommended direct references:

| Project | Package references |
|---|---|
| `Rasmus.SharedKernel` | `Megaraz.ResultPattern` |
| `media-vault-app.Application` | `Megaraz.ResultPattern` |
| `media-vault-app.Infrastructure` | All three packages |
| `media-vault-app.API` | Core and ASP.NET Core |
| Test projects | Packages corresponding to the code under test |

Do not add the ASP.NET Core package to Application solely for pagination.

The Phase 2 coexistence baseline implements this placement. `Rasmus.SharedKernel.Tests` directly references all three packages because it owns the cross-package characterization suite. `media-vault-app.Tests` directly references only the core package so reflection tests can prove Application contracts remain bound to the legacy type identity during coexistence.

### Temporary coexistence convention

- Production contracts and implementations continue to bind to `Rasmus.SharedKernel.ResultPattern` until their dedicated migration phases.
- Package types are exercised by characterization tests under explicit `Package...` aliases; legacy types use explicit `Legacy...` aliases whenever both identities appear in one file.
- Do not add a global using for either ResultPattern namespace while both implementations exist.
- A package reference does not authorize a piecemeal production type replacement. A contract and all of its in-scope implementations and consumers must move together in the appropriate child issue.
- Restore verification must use public NuGet explicitly at least once and inspect the resolved package graph before the local implementation is removed.

### First-party API and namespace consumers

- The React web client consumes safe top-level API `message` values through its transport/client layer. No first-party web path was found branching on ResultPattern error `code`.
- The Android client is TypeScript and consumes the HTTP contract; it does not reference the .NET SharedKernel assembly or namespace. Its separate TypeScript ResultPattern code retains legacy database-code names but is not a supported .NET consumer.
- Repository and GitHub code searches found no other .NET consumer of `Rasmus.SharedKernel.ResultPattern`, and the owner confirmed on 2026-08-01 that no supported external .NET consumer has been distributed or promised.
- D11 therefore permits removing the old namespace in Phase 6 after all in-repository callsites have migrated.

### Constructors and invariants

The local `ErrorContext` carries:

- Layer
- Service name
- Method name
- Operation
- Entity name
- Field name

The package constructor carries only:

- Operation
- Entity name
- Field name

The package rejects:

- Undefined operations.
- Blank entity names.
- Entity names containing `.`.
- Blank non-null field names.

All currently observed production entity names appear compatible, but the loss of layer, service, and method metadata is significant for diagnostics.

The local positional `Error` constructor is public. The package constructor is protected; arbitrary errors must use `Error.Custom`. The package also rejects blank codes and descriptions, undefined error types, and direct construction of validation errors.

### Result factories and generic constraints

- Package `Result<T>` requires `T : notnull`.
- Generic contracts may need explicit `notnull` constraints where no stronger constraint exists.
- Package `ValidationFailure` defaults `Message` to an empty string.
- The local implementation defaults it to `"Validation errors occurred, see validation errors for details."`
- `Result.Failure(error)` still uses `error.UserMessage`, but package factories leave `UserMessage` empty unless supplied.

A mechanical replacement would therefore create empty HTTP messages in many failure paths.

### Error types and pattern matching

The local `ErrorType` has distinct `Database` and `HttpError` members. The package replaces both with `ErrorType.External`, while concrete `DatabaseError` and `HttpError` types retain their detailed classifications.

Consequences:

- Existing enum switches must remove `Database` and `HttpError`.
- Logging policy should pattern-match concrete extension error types where required.
- Package HTTP mapping recognizes `HttpError` specially.
- Package default mapping treats other `External` errors, including `DatabaseError`, as HTTP 502.
- MediaVault currently returns HTTP 500 for database failures.

Recommended compatibility rule:

- Configure non-HTTP `External` failures as HTTP 500.
- Continue to let the package's special `HttpError` branch select HTTP-specific statuses.
- Document that this assumes all current non-HTTP external errors are internal failures.

### Error descriptions and user messages

The old implementation embeds layer, service, and method context in every technical description.

Package defaults are intentionally shorter:

- Core descriptions contain only the technical message.
- HTTP and database descriptions generally use `"Entity: description"`.
- User-facing messages are empty unless explicitly supplied.

The migration must explicitly distinguish:

- Stable public `Result.Message`.
- Per-field `ValidationError.UserMessage`.
- Technical `Error.Description`.
- Structured logging metadata.

Preserving HTTP behavior requires explicit user messages or MediaVault factory helpers. Preserving the exact old technical descriptions requires an application-owned diagnostic-origin strategy; the packages cannot do this by themselves.

### Error codes

Core conventional codes remain broadly compatible:

- `Create.User.Required`
- `Get.MediaEntry.NotFound`

Database codes change:

- Local: `Update.MediaEntry.DbSaveChangesFailure`
- Package: `Update.MediaEntry.DatabaseSaveChangesFailure`

The package `ErrorCode` no longer exposes `Operation`, `NameOfEntity`, or `Reason`; only the final `Code` is public.

Old database codes may already be observable in HTTP 500 bodies and log files. Their compatibility status must be decided before Phase 4.

### Validation

Package equivalents exist for required fields and matching values, but their safe default is an empty `UserMessage`. Using them mechanically would produce empty validation-item messages in MediaVault's HTTP responses.

Not provided by the package:

- Generic ID validity.
- Generic null checks.
- Integer minimum checks.

Recommended approach:

- Retain these as MediaVault-specific validation helpers outside the `ResultPattern` namespace.
- Have them create package `ValidationError` instances with explicit safe user messages.
- Remove `IsTooLow` if it remains unused and is not part of an external SharedKernel API contract.

### Exception handling

The packages do not map EF Core exceptions automatically. Existing repository catches remain MediaVault code.

The ASP.NET Core package adds `MapTransportExceptionToResult`, which distinguishes caller cancellation from timeout and transport failures. Current clients catch every `OperationCanceledException` and return `Error.Cancelled`, including some timeout cases.

Approved migration behavior:

- Caller-token cancellation propagates as cancellation, is not converted to a failed result, and is not logged as an unexpected failure.
- A non-caller `TaskCanceledException`, `TimeoutException`, or `HttpRequestException` becomes a logged `HttpErrorType.TransportFailure` with the fixed safe client message `"The external service is currently unavailable."`; API mapping remains HTTP 503.
- Repository cancellation behavior is unchanged during this migration.
- Cancellation behavior across the complete controller-to-database/outbound stack needs a separate future review; the package migration must not imply that broader review has happened.

### Outbound HTTP behavior

The package preserves:

- Web-default JSON deserialization.
- Missing-content-type leniency.
- Upstream error-message extraction.
- Status-to-`HttpErrorType` mapping.

It changes:

- Maximum inspected response body to 64 KiB by default.
- Oversized success bodies become `MalformedResponse`.
- Oversized error bodies fall back to a status-specific message.
- Extracted upstream text stays technical and is not copied to `UserMessage` unless `UserMessageFactory` opts in.

The current implementation exposes extracted upstream messages to clients. The approved policy intentionally fixes that oversight:

- Inspect at most 2 MiB (`2,097,152` bytes) from an upstream response body.
- Accept a body exactly at the limit and reject inspection at limit + 1 byte on both success and error paths.
- Retain bounded upstream text only in private technical descriptions used for diagnostics and logging.
- Never copy upstream text into `UserMessage` or the API response. Use MediaVault-owned fixed messages from `ExternalServiceResponsePolicy`.
- Oversized error bodies are not inspected and use the same fixed safe message policy.

The 2 MiB ceiling is deliberately above observed TMDB examples and bounded Google Books responses while accommodating RAWG searches at MediaVault's current page-size maximum. It is a safety ceiling, not a claim that providers guarantee responses below it.

### HTTP response mapping

Current validation response:

```json
{
  "message": "...",
  "validationErrors": [
    {
      "field": "...",
      "message": "..."
    }
  ]
}
```

Package default:

```json
{
  "message": "...",
  "code": "...",
  "validationErrors": [
    {
      "field": "...",
      "message": "..."
    }
  ]
}
```

This is a public API change. Preserve the old shape with `HttpResultMappingPolicy.FailureBodyFactory` unless the frontend is deliberately updated.

The package created-result extension accepts a literal location. MediaVault's adapter uses `CreatedAtAction` and route values. Keep a thin local adapter for this behavior.

### Pagination

The package pagination helper:

- Lives in the ASP.NET Core package.
- Requires an explicit `maxPageSize`.

The local helper:

- Lives in SharedKernel.
- Is used from Application.
- Defaults the maximum page size to 100.

Approved migration:

- `PaginationParameters` is MediaVault-owned under `Rasmus.SharedKernel.Pagination`.
- Preserve the current minimum of 1, default maximum of 100, and optional custom maximum.
- Keep normalization available to Application without introducing an ASP.NET Core dependency.

### Logging

No package provides the local logging subsystem.

Current behavior:

- NDJSON file named `errors.log.ndjson`.
- Seven-day default retention.
- Daily cleanup hosted service.
- Static asynchronous file lock.
- Corrupt lines skipped on read and removed during cleanup.
- Database errors generally logged.
- Validation and cancellation errors not logged.
- HTTP 400, 404, 409, and 422 not logged.
- HTTP authentication, rate-limit, server, transport, malformed, and unexpected errors logged.
- Logging failures are swallowed so they cannot change result flow.

Recommended placement:

- Keep logging contracts accessible from SharedKernel.
- Move the concrete file logger and extension-aware logging policy to Infrastructure.
- Do not move the contracts into Infrastructure, because `ErrorLogCleanupService` in Application would create an Application-to-Infrastructure cycle.
- Replace the legacy multiline origin formatting with the package error description plus a MediaVault-owned `ErrorLogContext` containing structured `Layer`, `Service`, and `Method` fields.
- Existing NDJSON records do not need backward-compatible reading. The schema change occurs with the logger migration, not in the coexistence baseline.
- Retain the current seven-day default retention, daily cleanup, asynchronous file locking, corrupt-line handling, and failure isolation.

## Mechanical replacements

The following changes are primarily mechanical after compatibility decisions are settled:

- Add pinned package references.
- Change core namespaces.
- Change database and HTTP namespaces.
- Replace `.From<TIn,TOut>()` with `.ToResult<TOut>()`.
- Retain existing `Map` calls.
- Change `ErrorContext` construction to `(operation, entityName, fieldName)`.
- Add required `notnull` constraints.
- Replace direct test construction of `Error` with `Error.Custom`.
- Update database and HTTP top-level error assertions to `ErrorType.External`.
- Update DI namespaces after relocating the logger implementation.
- Remove tests that only duplicate package implementation tests.

## Resolved compatibility decisions

All compatibility decisions were presented individually to and approved by the repository owner on 2026-08-01. Later migration issues implement these decisions; they must not reopen or silently reinterpret them without recording a superseding owner decision here.

## Decision register

This table records the owner-approved Phase 2 baseline. Do not silently reinterpret these resolutions in later code.

| ID | Decision | Recommended default | Status | Resolution |
|---|---|---|---|---|
| D1 | Validation response includes top-level `code` | Preserve current shape initially | Approved | Keep `{ message, validationErrors }` with no top-level or item `code`; validation codes remain internal diagnostics. |
| D2 | Database error-code format | Preserve externally observable codes unless confirmed unused | Approved | Adopt package-native `Database...` code suffixes. No first-party client branches on database codes; this is an intentional diagnostic-contract change. |
| D3 | Core and validation user messages | Preserve current safe client messages explicitly | Approved | Preserve current safe workflow, core, and validation wording explicitly when creating package errors; do not accept empty package defaults. Editorial cleanup is separate work. |
| D4 | Upstream error text exposure | Use fixed or sanitized allowlisted messages | Approved | Upstream text is untrusted and never user/API-visible. Retain bounded text only in private descriptions/logs and return MediaVault-owned fixed messages. |
| D5 | Maximum upstream response body | Configure explicitly after checking realistic RAWG/TMDB/Books sizes | Approved | Use exactly 2 MiB (`2,097,152` bytes), accepting the exact boundary and rejecting boundary + 1 on success and error paths. |
| D6 | Layer/service/method diagnostics | Preserve as structured logging metadata if needed; do not force it into package types | Approved | Add MediaVault-owned `ErrorLogContext(Layer, Service, Method)` and combine it with the package description. Discard the legacy multiline format and old-record compatibility. |
| D7 | Non-HTTP `External` HTTP status | Map to 500 | Approved | Pattern-match concrete `HttpError` first; map every other current `External` to 500. Future external types require an explicit mapping decision. |
| D8 | Pagination placement | Keep a neutral SharedKernel helper with max 100 | Approved | Keep MediaVault-owned normalization in `Rasmus.SharedKernel.Pagination`, preserving min 1, default max 100, and optional custom max without an Application ASP.NET dependency. |
| D9 | Cancellation and timeout semantics | Preserve current behavior during migration | Approved with change | Propagate caller cancellation without logging; map non-caller task cancellation, timeout, and transport failure to logged `TransportFailure`, safe fixed text, and HTTP 503. Keep repository cancellation unchanged and review full-stack cancellation separately. |
| D10 | File logger | Retain and relocate implementation to Infrastructure | Approved | Retain NDJSON behavior and neutral contracts; later move concrete configuration, persistence, and extension-aware policy to Infrastructure with the D6 schema. Observability replacement is separate. |
| D11 | External consumers of old namespace | Confirm none before removal | Approved | Owner confirmed no supported external .NET consumer. Web and Android consume HTTP only; Android's similarly named TypeScript code is independent. Remove the old namespace only after repository callsites migrate. |

## Incremental implementation plan

### Phase 1 — Inventory and dependency analysis

**Status:** Complete. Baseline on 2026-08-01: clean restore/build, 292 SharedKernel tests and 103 application tests passed before the coexistence changes.

**Objective:** Freeze the current scope and contract surface.

**Affected projects:** All backend projects and both test projects.

**Affected files or symbols:** The inventory above, project references, controller adapter calls, and ResultPattern tests.

**Implementation tasks:**

- Record the local implementation and retained non-package behaviors.
- Record direct and transitive Result-typed contracts.
- Capture current HTTP examples, error codes, messages, log records, and status mappings.
- Confirm the working tree state before implementation.
- Avoid frontend or unrelated architecture changes.

**Expected risks:**

- Missing a transitive public contract, especially SharedKernel generic interfaces or controller extension calls.

**Verification:**

- Search for all local namespace imports and Result-related symbols.
- Confirm every backend and test project appears in the dependency matrix.
- Establish a clean baseline build and test result once implementation is authorized.

**Suggested checkpoint:**

- `docs: inventory ResultPattern migration surface`

**Dependencies:** None.

### Phase 2 — Migration mapping and compatibility decisions

**Status:** Complete under issue #90 on 2026-08-01.

**Objective:** Resolve compatibility behavior before changing type identity.

**Affected projects:** SharedKernel, Application, Infrastructure, API, and tests.

**Affected files or symbols:** Project files, characterization tests, response contracts, and logging policy tests.

**Implementation tasks:**

- Pin package versions `0.2.2`, `0.1.1`, and `0.1.0`.
- Add package references without removing the local implementation.
- Add or update characterization tests for:
  - HTTP statuses and JSON shapes.
  - Validation field messages.
  - Database and HTTP error codes.
  - Cancellation.
  - Upstream error messages.
  - Logging policy and NDJSON schema.
  - Oversized upstream responses.
- Resolve every open item in the decision register.
- Introduce package-agnostic MediaVault policies and helpers where needed:
  - Explicit user-message factories.
  - Neutral identifier and null validation helpers.
  - Central HTTP mapping policy.
  - Configured response-body limit.
- Document that package and local types coexist temporarily under different namespaces.

**Implemented baseline:**

- Added exact direct package references according to the dependency matrix while retaining every legacy type and production call path.
- Added explicit alias-based coexistence tests and contract tests for HTTP status/body/`Location`, validation messages and JSON, legacy and package database codes, safe upstream diagnostics, exact 2 MiB boundaries, cancellation/transport classification, logging policy, and NDJSON schema.
- Added package-agnostic `ExternalServiceResponsePolicy` and `ErrorLogContext` seams.
- Relocated pagination to `Rasmus.SharedKernel.Pagination` without changing normalization behavior.
- Resolved D1-D11 with owner approval. The remaining phases implement those locked decisions.
- Final Phase 2 verification passed 320 SharedKernel tests and 104 application tests. The full build retained the nine pre-existing package-advisory warnings and introduced no new warning category or occurrence.

**Expected risks:**

- Ambiguous type names.
- Tests binding to the wrong namespace.
- Accidentally treating changed public behavior as a mechanical update.

**Verification:**

- Restore and build with both implementations present.
- Run all tests.
- Confirm characterization tests fail when selected compatibility behavior is intentionally perturbed.

**Suggested checkpoint:**

- `test: characterize ResultPattern migration contracts`

**Dependencies:** Phase 1.

### Phase 3 — Core ResultPattern migration

**Status:** Complete under issue #91 on 2026-08-01.

The SharedKernel and Application public contracts now expose `Megaraz.ResultPattern` core types, and their Infrastructure, API, and test callers use the same identities. MediaVault-owned error-message, validation, pagination, logging-context, and HTTP-response policies remain explicit. The still-local database/HTTP errors and mapper are isolated under `Rasmus.SharedKernel.ResultPatternCompatibility` as a narrow bridge for Phases 4 and 5; Phase 6 removes that bridge and the legacy implementation.

**Objective:** Change the backend's core result, error, and validation type identity to `Megaraz.ResultPattern`.

**Affected projects:** SharedKernel, Application, Infrastructure, API, and both test projects.

**Affected files or symbols:**

- Thirteen SharedKernel contracts.
- Eleven Application interfaces.
- Generic service and repository bases.
- Validators and Application services.
- `Result`, `Error`, `ErrorContext`, `ValidationError`, and `OperationType`.
- `.Map` and `.From` propagation sites.

**Implementation tasks:**

- Migrate SharedKernel public contracts to package core types.
- Migrate Application interfaces and implementations together so return types always match.
- Replace `.From<TIn,TOut>()` with `.ToResult<TOut>()`.
- Add missing `notnull` constraints.
- Replace six-argument `ErrorContext` construction with the package constructor.
- Use MediaVault validation helpers where explicit user messages or unsupported checks are required.
- Convert `ErrorReasonCode` usage to `ErrorCodeReasons` or caller-owned strings.
- Temporarily adapt still-local database and HTTP errors/mappers to consume package core `Error` and `Result`.
- Update Application tests to package types while retaining approved behavior assertions.

The temporary compatibility bridge is required if Phases 4 and 5 are to remain independently buildable. It must be removed in Phase 6.

**Expected risks:**

- Assembly-identity mismatch between local and package types.
- Nullable-generic warnings becoming errors.
- Empty result or validation messages.
- Lost diagnostic context.
- Ambiguous imports.

**Verification:**

- Build the full solution.
- Run SharedKernel and Application tests.
- Search production code for local core `Result`, `Error`, `ErrorContext`, and `ValidationError`.
- Confirm interfaces and implementations use the same package types.

**Suggested checkpoint:**

- `refactor: migrate core result contracts to Megaraz.ResultPattern`

**Dependencies:** Phases 1 and 2.

### Phase 4 — Infrastructure and database-error migration

**Status:** Complete via issue #93 (pending review).

**Objective:** Replace the local database error model while preserving repository behavior.

**Affected projects:** Infrastructure, SharedKernel logging support, API mapping policy, and tests.

**Affected files or symbols:**

- `media-vault-app.Infrastructure/Repos/RepoBase.cs`
- `media-vault-app.Infrastructure/Repos/DependentEntityRepoBase.cs`
- `media-vault-app.Infrastructure/Repos/UserRepo.cs`
- `media-vault-app.Infrastructure/Repos/MediaEntryRepo.cs`
- Logger and logging policy types.

**Implementation tasks:**

- Reference `Megaraz.ResultPattern.Infrastructure`.
- Replace local database error imports and factories.
- Keep EF exception catch ordering unchanged:
  - cancellation;
  - concurrency;
  - update failure;
  - query or unexpected failure.
- Pass explicit safe user messages where required.
- Apply the chosen database-code compatibility strategy.
- Relocate the concrete file logger and `ErrorLogPolicy` to Infrastructure.
- Keep logging contracts in SharedKernel.
- Ensure the HTTP policy maps non-HTTP `External` failures to 500.
- Preserve logging-failure swallowing.
- Do not fix the SQL Server 2601/2627 uniqueness check while SQLite is configured as part of this migration; record it as unrelated follow-up work.

**Expected risks:**

- Database failures changing from 500 to 502.
- Changed error codes.
- Empty response messages.
- Logging omissions in derived repositories.
- SharedKernel accidentally referencing the Infrastructure package.

**Verification:**

- Add or run repository exception-mapping tests for every database category.
- Assert `DatabaseError.Type == ErrorType.External`.
- Assert approved code and message compatibility.
- Assert database failures still map to HTTP 500.
- Verify NDJSON records retain exception message and stack trace.
- Confirm SharedKernel references only the core package.

**Completed implementation notes:**

- Infrastructure now creates package-native `DatabaseError` values through a MediaVault-owned safe-message policy; package-native `Database...` code suffixes are intentional.
- Concrete NDJSON persistence and extension-aware logging classification now live in Infrastructure. SharedKernel retains only neutral `IErrorLogger`, `IErrorLogPolicy`, `ErrorLog`, and `ErrorLogContext` contracts so Application cleanup remains dependency-safe.
- The HTTP bridge relies on the approved generic `ErrorType.External` 500 mapping; it no longer names a local database error type.

**Suggested checkpoint:**

- `refactor: adopt Megaraz database errors and relocate logging implementation`

**Dependencies:** Phase 3.

### Phase 5 — ASP.NET Core and HTTP-mapping migration

**Status:** In progress. Outbound mapping completed under issue #92; inbound API mapping remains under issue #94.

**Objective:** Replace outbound HTTP conversion and inbound API response mapping.

**Affected projects:** Infrastructure HTTP clients, API, and HTTP-related tests.

**Affected files or symbols:**

- Three external API clients.
- `ApiClientBase`.
- `ResultResponseMapper`.
- All six controllers indirectly.
- Local HTTP error, response conversion, mapper, and response DTO files.
- Pagination callsites.

**Implementation tasks:**

- Reference `Megaraz.ResultPattern.AspNetCore` from Infrastructure and API.
- Replace local `HttpError` and `MapToResultAsync`.
- Configure `HttpResponseMappingOptions` with:
  - an explicit maximum body size;
  - the approved upstream message extractor;
  - a vetted `UserMessageFactory`, if upstream messages remain client-visible.
- Preserve current catch behavior unless timeout semantics were explicitly approved for change.
- Define one MediaVault `HttpResultMappingPolicy`:
  - existing status mappings;
  - non-HTTP `External` to 500;
  - approved validation response shape;
  - safe failure bodies.
- Keep `ResultResponseMapper` as a thin adapter:
  - delegate ordinary success and failure mapping to the package;
  - retain `CreatedAtAction(actionName, routeValues, value)`.
- Avoid importing both competing `ToActionResult` extensions into controllers while both exist.
- Resolve pagination according to D8.
- Update HTTP mapper and client tests.

**Implemented outbound slice (#92):**

- All Google Books, RAWG, and TMDB responses flow through one Infrastructure-owned package mapping path.
- `HttpResponseMappingOptions.MaxResponseBodyBytes` is explicitly set to the approved 2 MiB ceiling; exact-boundary and boundary-plus-one tests cover success and error bodies.
- Package web-default JSON handling and missing-content-type compatibility are retained.
- Bounded provider text remains in the technical error description, while every result message is replaced with the fixed `ExternalServiceResponsePolicy` message for the response status.
- Caller cancellation propagates without logging; non-caller task cancellation, timeout, and transport failures become one logged `TransportFailure` result with the fixed safe message.
- The existing API adapter has a narrow migration bridge for package `HttpError` status selection, preserving the current response body and HTTP 503 transport contract until issue #94 replaces the inbound mapper.
- Provider endpoints, DTOs, authentication, inbound API mapping, persistence, and first-party client contracts are unchanged.

**Expected risks:**

- Public JSON shape changes.
- Empty messages.
- Oversized legitimate search responses rejected at 64 KiB.
- Upstream text exposure.
- Database failures becoming 502.
- Extension-method ambiguity.
- Accidental ASP.NET Core dependency in Application.

**Verification:**

- Contract tests for 200, 201, 204, 400, 401, 403, 404, 409, 422, 429, 500, 502, and 503.
- Snapshot validation and ordinary error JSON.
- Test missing, invalid, oversized, and non-JSON upstream bodies.
- Test caller cancellation separately from timeout and transport failure.
- Verify `CreatedAtAction` still produces the expected `Location`.
- Verify all external clients propagate and log approved errors.

**Suggested checkpoint:**

- `refactor: adopt Megaraz ASP.NET Core result mapping`

**Dependencies:** Phases 3 and 4.

### Phase 6 — Remove the old implementation

**Status:** Not started.

**Objective:** Eliminate the internal ResultPattern implementation without deleting retained MediaVault behavior.

**Affected projects:** SharedKernel, Infrastructure, API, and both test projects.

**Affected files or symbols:** The `Rasmus.SharedKernel/ResultPattern` folder and obsolete tests.

**Implementation tasks:**

- Ensure retained helpers have moved to non-ResultPattern namespaces:
  - identifier and null validation;
  - pagination if retained;
  - file logging;
  - HTTP contract adapter.
- Delete old core, database, HTTP, mapper, and response DTO implementations.
- Remove the temporary compatibility bridge.
- Remove or rewrite tests that duplicate package unit tests.
- Retain tests for MediaVault-owned behavior:
  - message policy;
  - validation helpers;
  - logging;
  - HTTP response contract;
  - repository exception translation.
- Remove obsolete imports, aliases, comments, and implementation documentation.
- Search for the old namespace and local type definitions.

**Expected risks:**

- Deleting logging or validation behavior not supplied by the packages.
- Tests passing against stale build output.
- Hidden namespace references in generic contracts.
- Breaking consumers outside this solution.

**Verification:**

- Clean build from restored packages.
- No production reference to `Rasmus.SharedKernel.ResultPattern`.
- No locally declared `Result`, `Error`, `ValidationError`, `DatabaseError`, `HttpError`, or `HttpResultMapper`.
- Package references resolve to the pinned published versions.

**Suggested checkpoint:**

- `refactor: remove legacy MediaVault ResultPattern implementation`

**Dependencies:** Phases 3 through 5.

### Phase 7 — Final verification, cleanup, and documentation

**Status:** Not started.

**Objective:** Prove behavioral compatibility and leave the dependency model explicit.

**Affected projects:** Entire backend and documentation.

**Implementation tasks:**

- Restore, build, and test the full solution from a clean state.
- Run focused repository, HTTP-client, controller-mapping, validation, logging, and cancellation tests.
- Inspect resolved direct and transitive package versions.
- Review generated OpenAPI response schemas.
- Compare representative pre- and post-migration HTTP payloads and log records.
- Confirm no frontend change is needed unless an API change was explicitly approved.
- Update backend documentation with:
  - package versions;
  - namespace ownership;
  - error-message policy;
  - database and HTTP status policy;
  - response-body limit;
  - retained MediaVault helpers;
  - logger ownership.
- Remove only migration-specific temporary code and comments.

**Expected risks:**

- Stale outputs.
- Incomplete contract comparison.
- Undocumented public API changes.

**Verification:**

- No build warnings introduced by generic constraints or nullable annotations.
- All tests pass.
- Old namespace search is empty outside historical documentation.
- API and logging behavior match the decision register.
- SharedKernel and Application have no ASP.NET Core or database package dependency.

**Suggested checkpoint:**

- `test: verify ResultPattern package migration`

**Dependencies:** All previous phases.

## Recommended execution order

1. Establish a clean baseline and preserve any unrelated working-tree changes.
2. Add package references and characterization tests.
3. Resolve the decision register.
4. Migrate core type identity across contracts and implementations in one coordinated checkpoint.
5. Migrate database errors and relocate logging implementation.
6. Migrate outbound HTTP conversion and API response mapping.
7. Remove the local implementation and temporary bridges.
8. Run clean full-solution verification and documentation review.

Within each implementation phase, migrate vertical slices together—contract, implementation, caller, and tests—rather than leaving incompatible local and package Result types across an interface boundary.

## Prioritized risk assessment

### Critical

- Empty client messages after mechanical factory replacement.
- Database errors changing from HTTP 500 to 502.
- Validation response schema changing unexpectedly.
- Mixing local and package Result/Error type identities across interfaces.

### High

- Database error-code changes breaking clients or diagnostics.
- Loss of layer, service, and method logging context.
- A later outbound adapter failing to configure the approved 2 MiB body limit on every relevant path.
- Returning untrusted upstream error text if compatibility is implemented unsafely.
- Missing `notnull` constraints on generic public APIs.

### Medium

- Application acquiring an unwanted ASP.NET Core dependency through pagination.
- Cancellation and timeout behavior changing.
- `CreatedAtAction` becoming plain `Created`.
- Logging policy regressions after both extension errors become `External`.
- Tests asserting package-internal descriptions rather than MediaVault contracts.

### Low

- `Error.ToString()` formatting change.
- Removal of structured `ErrorCode.Operation`, `NameOfEntity`, and `Reason`.
- Removal of unused `IsTooLow` or legacy error-reason members.
- Record sealing or direct-constructor changes where production does not subclass or construct the types.

## Definition of Done

The migration is complete when:

- [ ] The backend uses the three pinned published packages for core, database, and HTTP ResultPattern types.
- [ ] No local ResultPattern implementation or old namespace remains.
- [ ] SharedKernel depends only on the core package.
- [ ] Application has no ASP.NET Core or database package dependency.
- [ ] MediaVault-owned validation, pagination, HTTP contract, and logging behavior is retained in clearly named non-package components.
- [ ] Every project builds from a clean restore.
- [ ] All tests pass.
- [ ] HTTP statuses, JSON shapes, error codes, user messages, cancellation behavior, and logs match the decision register.
- [ ] No temporary adapters remain.
- [ ] Tests no longer duplicate package implementation tests.
- [ ] Package ownership and application-specific policies are documented.

## Change log

| Date | Change |
|---|---|
| 2026-07-28 | Created the source-of-truth document from the completed repository and package analysis. |
| 2026-08-01 | Completed Phases 1-2 under issue #90: pinned public packages for coexistence, added characterization coverage and MediaVault-owned policy seams, and recorded owner-approved resolutions for D1-D11. |
| 2026-08-01 | Completed Phase 3 under issue #91: migrated backend contracts and callers to package core types, preserved MediaVault-owned policies, and isolated the temporary database/HTTP compatibility bridge. |
