# Error handling and observability verification

**Issue:** [#113](https://github.com/Megaraz/MediaVault.Api/issues/113)  
**Verified:** 2026-08-14  
**Scope:** API, Application, Infrastructure, SharedKernel, both backend test
projects, public error contracts, package direction, and local Aspire guidance

## Outcome

The integrated foundation preserves expected `Result` contracts, gives
unexpected exceptions one safe correlated boundary, emits classified standard
logs and vendor-neutral OpenTelemetry signals, and contains no custom NDJSON
sink or cleanup service.

The clean verification exposed one integration defect: the host-default Windows
Event Log provider could throw when it lacked access to its event source, so a
framework warning failed five request-level telemetry tests. The API composition
root now registers only the supported console and debug providers before adding
OpenTelemetry. Logging configuration still controls levels, while an
unsupported host provider can no longer enter the pipeline implicitly.

## Automated evidence

The repository verification commands are:

```powershell
dotnet clean media-vault-app.slnx
dotnet restore media-vault-app.slnx
dotnet build media-vault-app.slnx --no-restore
dotnet test media-vault-app.slnx --no-build --no-restore
dotnet list media-vault-app.slnx package --include-transitive
rg -n "IErrorLogger|IErrorLogPolicy|LogErrorToFileAsync|ErrorLogCleanupService|errors\.log\.ndjson" `
    -g '*.cs' -g '*.csproj' -g '*.json'
git diff --check
```

Focused suites cover these contracts:

| Flow | Expected result and diagnostic owner |
| --- | --- |
| Validation, not found, conflict | Existing safe Result/status/body; no Warning/Error event by default |
| Database concurrency or known failure | Safe Result; Infrastructure event 2000 or 2001 exactly once |
| Upstream timeout, transport, 429, or 5xx | Safe 503 Result; Infrastructure event 2100 exactly once |
| Upstream authentication or malformed content | Safe Result; event 2101 or 2102 exactly once without raw content |
| Caller cancellation | Cancellation remains cancellation; no event 3000 and no synthetic 500 |
| Unexpected exception | Safe `application/problem+json` 500 with `traceId`; API event 3000 exactly once |
| Receiver unavailable | Request outcome is unchanged; telemetry may be dropped |

`OpenTelemetryTests`, `ExceptionBoundaryTests`, `DatabaseAndLoggingTests`,
`ErrorEventLoggerTests`, `ExternalApiClientTests`, controller response metadata,
generated OpenAPI contract, and ResultPattern compatibility tests provide the
executable evidence. The telemetry tests assert trace parentage, log/trace
correlation, bounded metric
dimensions, query-value redaction, non-Development exception sanitization, and
receiver-outage isolation.

The clean baseline still reports only the documented low-severity package
advisories for `NuGet.Packaging` and `NuGet.Protocol` 6.12.1 from the
design-time scaffolding dependency graph. The high-severity
`Microsoft.OpenApi` and `SQLitePCLRaw.lib.e_sqlite3` advisories, plus the
test-project EF Core 10.0.9/10.0.10 assembly-version conflict warning, are
remediated by the backend dependency alignment documented in
`docs/dependency-advisory-baseline.md`.

## Contract and privacy review

- Expected failures retain their approved status codes, content types, bodies,
  and headers; this issue changes no route, authentication, authorization,
  ownership, persistence, web, or Android contract.
- Unexpected responses contain only the stable ProblemDetails fields and a
  usable trace identifier. Exception messages, types, stacks, SQL, credentials,
  provider bodies, reviews, email addresses, tokens, and passwords do not enter
  public responses or exported non-Development events.
- Domain and Application have no OpenTelemetry/exporter dependency. The API
  owns providers and exporters; Infrastructure owns database/provider event
  classification.
- Repository search finds no production contract, registration, file side
  effect, or test for `IErrorLogger`, `IErrorLogPolicy`,
  `LogErrorToFileAsync`, `ErrorLogCleanupService`, or `errors.log.ndjson`.

## Local Aspire workflows

The primary workflow is the Aspire AppHost in the
[`Megaraz/MediaVault`](https://github.com/Megaraz/MediaVault) workspace. Its
presets orchestrate the API alone or together with web and Android clients and
open the local dashboard. The API's backend-only
[standalone dashboard guide](standalone-aspire-dashboard.md) remains a supported
alternative and a deterministic test-export path.

In either workflow, inspect one healthy request, expected 404, provider failure,
and controlled test exception across Structured logs, Traces, and Metrics. Stop
the dashboard/receiver and repeat a healthy request; it must still succeed.
Dashboard observations are manual evidence because the UI is ephemeral, while
the same signal shapes and receiver-outage behavior are asserted in memory by
the automated suite.

## Remaining boundaries

This foundation does not implement or imply:

- resilience policy, retries, explicit timeout budgets, or rate limiting
  ([#64](https://github.com/Megaraz/MediaVault.Api/issues/64));
- production hosting, telemetry storage, retention, alerting, cost budgets, or
  a production APM ([#69](https://github.com/Megaraz/MediaVault.Api/issues/69));
- offline synchronization and conflict policy
  ([#65](https://github.com/Megaraz/MediaVault.Api/issues/65));
- AI recommendations, provider selection, privacy/cost controls, or fallback
  behavior ([#68](https://github.com/Megaraz/MediaVault.Api/issues/68)).

The local AppHost is implemented development orchestration, not evidence that
production hosting or production observability is complete.
