# OpenTelemetry baseline

## Status and boundary

Issue #111 established the vendor-neutral telemetry baseline for the ASP.NET Core API. The API composition root owns OpenTelemetry SDK, instrumentation, resource, sampling, filtering, and exporter configuration. Domain and Application contain no OpenTelemetry or exporter dependency, and public API/auth/data/client contracts are unchanged.

The baseline uses only built-in ASP.NET Core, `HttpClient`, and .NET runtime instrumentation. No custom `ActivitySource` or `Meter` is justified yet because the named questions below are answered without adding application-specific dimensions or lifecycle ownership.

## Operational questions

1. **Are API requests healthy?** Incoming request spans and `http.server.request.duration` show route-template, method, status, latency, and error trends.
2. **Which outbound provider is slow or failing?** `HttpClient` spans and `http.client.request.duration` show dependency host, method, status/error type, and latency while preserving trace parentage.
3. **Is runtime pressure contributing to failures?** The stable `System.Runtime` meter exposes .NET 10 process/runtime signals for GC, allocation, memory, threads, exceptions, and JIT behavior.
4. **Which approved MediaVault event occurred in a failing request?** Exported Warning/Error records from `media_vault_app.*` retain their structured event identity and active trace/span identifiers.

## Packages and ownership

All production packages are stable OpenTelemetry 1.17.0 releases compatible with .NET 10:

- `OpenTelemetry.Extensions.Hosting` owns SDK/provider lifetime;
- `OpenTelemetry.Instrumentation.AspNetCore` collects incoming traces and request metrics;
- `OpenTelemetry.Instrumentation.Http` collects outbound traces, propagation, and HTTP metrics;
- `OpenTelemetry.Instrumentation.Runtime` subscribes to .NET 10's built-in `System.Runtime` metrics;
- `OpenTelemetry.Exporter.OpenTelemetryProtocol` supplies vendor-neutral OTLP export.

`OpenTelemetry.Exporter.InMemory` is test-only. The standalone Process instrumentation package was not added because its current release is prerelease and .NET 10 runtime instrumentation already exposes the required stable process/runtime baseline.

## Resource and configuration

Configuration is read from the typed `OpenTelemetry` section. Standard ASP.NET Core configuration precedence applies, so environment variables use double underscores.

| Setting | Default | Purpose |
| --- | --- | --- |
| `OpenTelemetry__Enabled` | `true` | Enables SDK collection. Set `false` to remove providers/instrumentation. |
| `OpenTelemetry__OtlpExporterEnabled` | `false` | Enables OTLP export for logs, traces, and metrics. Requires telemetry to be enabled. |
| `OpenTelemetry__ServiceName` | `MediaVault.API` | Stable `service.name`; must not contain host/user identity. |
| `OpenTelemetry__ServiceVersion` | entry assembly version | Stable deployed build version; may be set by the hosting environment. |
| `OpenTelemetry__Environment` | ASP.NET Core environment name | `deployment.environment.name`, such as Development, Staging, or Production. |
| `OpenTelemetry__TraceSamplingRatio` | `0.1` (`1.0` in Development) | Parent-based trace-ID ratio from 0 through 1. Metrics and logs are not trace-sampled. |

When OTLP is enabled, the exporter reads standard OpenTelemetry variables:

```text
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
OTEL_EXPORTER_OTLP_PROTOCOL=grpc
OTEL_EXPORTER_OTLP_TIMEOUT=10000
```

Signal-specific endpoint/protocol/header variables are also supported. Authentication headers are secrets and belong in environment/secret storage, never committed configuration. The exact local workflow is documented in [Standalone Aspire Dashboard](standalone-aspire-dashboard.md).

## Volume, filtering, and cardinality

- Base configuration samples 10% of new root traces and respects the parent decision. Development samples all traces for local diagnosis.
- OTLP logs are limited to categories beginning `media_vault_app.` at Warning or above. Existing console/debug provider behavior remains governed by normal `Logging` configuration.
- Request metrics use normalized route templates rather than user/media identifiers. Outbound metrics use bounded standard HTTP attributes.
- No custom user ID, media ID, review, provider payload, exception-message, or free-form error attribute is added.
- A future production deployment must choose sampling and cost budgets from measured traffic; this baseline does not claim a production retention or alerting policy.

## Redaction and exception ownership

- Request/response bodies and headers are not captured.
- ASP.NET Core and `HttpClient` query-value redaction remains enabled. Do not set the experimental `*_DISABLE_URL_QUERY_REDACTION` variables to `true`.
- Authorization headers, JWTs, API keys, provider tokens, raw upstream bodies, reviews, SQL, and personal data are forbidden.
- Automatic ASP.NET Core and `HttpClient` exception recording is disabled. Known database/upstream events retain their existing single owner, and `MediaVaultExceptionHandler` alone owns event 3000.
- Non-Development event producers continue to omit exception objects/messages/stacks before records enter the telemetry pipeline. Development may retain local exception detail under the approved policy.

## Exporter failure behavior

OTLP exporters use asynchronous batch processors/readers and are not awaited by application requests. Connection refusal, timeout, or receiver outage can drop telemetry but must not change an HTTP outcome. Focused integration coverage sends telemetry to an unreachable loopback endpoint, forces exporter flushes, and verifies the application request still succeeds.

Telemetry failure is operationally relevant but is not a reason to retry application writes or recreate application-owned telemetry storage. OpenTelemetry self-diagnostics or collector monitoring should own exporter health in a future hosting design.

## Verification

`OpenTelemetryTests` deterministically verifies:

- incoming and real outbound HTTP trace parentage;
- Warning log correlation with the incoming trace;
- success, expected-failure, outbound-failure, and unexpected-exception shapes;
- outbound query-value redaction and secret absence;
- server, client, and `System.Runtime` metric availability;
- absence of custom user/media metric dimensions;
- service name/version/environment resource attributes;
- OTLP receiver-outage isolation.

For hands-on verification, setting `MEDIAVAULT_TEST_OTLP_EXPORT=true` adds the
configured OTLP exporter to the same deterministic success, expected-failure,
outbound-failure, and unexpected-exception scenarios. It does not add test
routes to the production API. See the standalone dashboard guide for the exact
command and safe environment cleanup.

Repository verification remains:

```powershell
dotnet restore media-vault-app.slnx
dotnet build media-vault-app.slnx
dotnet test media-vault-app.slnx
dotnet list media-vault-app.slnx package --include-transitive
git diff --check
```

## References

- [.NET observability with OpenTelemetry](https://learn.microsoft.com/dotnet/core/diagnostics/observability-with-otel)
- [OpenTelemetry .NET OTLP exporter](https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/src/OpenTelemetry.Exporter.OpenTelemetryProtocol)
- [OpenTelemetry .NET exporters](https://opentelemetry.io/docs/languages/dotnet/exporters/)
- [Built-in .NET metrics](https://learn.microsoft.com/dotnet/core/diagnostics/built-in-metrics)
