# Standalone Aspire Dashboard (alternative workflow)

## Purpose and boundary

MediaVault's primary local development entry point is the Aspire AppHost in the
[`Megaraz/MediaVault`](https://github.com/Megaraz/MediaVault) workspace. Run its
`run.ps1` and choose the API, web, Android, or all preset to start the selected
resources and open the dashboard.

This document preserves the optional backend-only workflow: the API exports
local OpenTelemetry logs, traces, and metrics to a standalone Aspire Dashboard.
The standalone process does not orchestrate the API, web client, database, or
external providers. The API starts and serves requests when either dashboard is
stopped or unavailable; telemetry may be dropped in that case.

The dashboard is a development and short-term diagnostic viewer. It keeps
telemetry in memory, evicts data when its limits are reached, and loses all data
when it restarts. It is not durable storage, alerting, retention, or the future
production observability solution.

## Prerequisites and pinned tool

The primary workflow uses `npx`, which is included with the repository's
supported Node.js/npm setup. It does not require a global Aspire installation
or Docker.

Commands pin `@microsoft/aspire-cli` to `13.4.6`, the stable version verified
for this guide. Do not silently replace the version with `latest`: review
Aspire release notes and repeat this guide when updating it.

Verify the package before starting:

```powershell
npx -y @microsoft/aspire-cli@13.4.6 --version
```

The expected version begins with `13.4.6`.

## Start and sign in

From any terminal, start the standalone dashboard:

```powershell
npx -y @microsoft/aspire-cli@13.4.6 dashboard run
```

The command binds the browser UI to `http://localhost:18888`, OTLP/gRPC to
`http://localhost:4317`, and OTLP/HTTP to `http://localhost:4318`. Keep this
terminal running.

Browser access is protected by default. Open the transient login URL printed in
the terminal; it has the form `http://localhost:18888/login?t=<token>`. Treat
that URL as a local secret: do not commit it, paste it into issues or logs, or
include it in screenshots.

Standalone OTLP ingestion is unsecured by default, but these commands bind it
to localhost. Do not expose ports 4317 or 4318 to another machine or untrusted
network. Securing a remotely reachable collector requires an explicit API-key
or certificate design and secret-backed exporter headers; it is outside this
local workflow.

Aspire also supports `--allow-anonymous`, but that disables browser
authentication. Use it only for a deliberate, short-lived session on a trusted
local machine. It is not the primary MediaVault command and must never be
copied into production configuration.

## Run MediaVault with local OTLP export

Configure the API's normal local user secrets as described in the repository
README. Then, from the repository root, run the explicit OTLP launch profile:

```powershell
dotnet run --project media-vault-app.API --launch-profile http-otel
```

The `http-otel` profile is Development-only convenience configuration. It
enables all three exporters and selects OTLP/gRPC at
`http://localhost:4317`. It contains no token or machine-specific path. The
resource identity remains bounded and stable:

- `service.name`: `MediaVault.API`
- `service.version`: the API assembly version
- `deployment.environment.name`: `Development`
- root trace sampling: `1.0` in Development

For a quick healthy request and to confirm that no public user-management
surface is exposed:

```powershell
Invoke-WebRequest http://localhost:5210/openapi/v1.json
Invoke-WebRequest http://localhost:5210/Users `
    -SkipHttpErrorCheck
```

The first request returns 200. The second request returns 404 because the
anonymous user-management route was removed. Authenticated user access is
available through `/Auth/me`.

Use normal authenticated provider search through the web client to see a real
outbound `HttpClient` child span. Do not intentionally consume provider quota
or change credentials merely to manufacture a failure.

## Reproduce all representative signal shapes

The focused integration harness is the safe, deterministic way to create the
success, expected Result failure, upstream failure, and unexpected exception
required by issue #112. Its controller and loopback upstream exist only in the
test assembly; no diagnostic or exception endpoint is exposed by the running
API.

In a new PowerShell terminal at the repository root:

```powershell
$env:MEDIAVAULT_TEST_OTLP_EXPORT = "true"
$env:OTEL_EXPORTER_OTLP_ENDPOINT = "http://localhost:4317"
$env:OTEL_EXPORTER_OTLP_PROTOCOL = "grpc"

try {
    dotnet test media-vault-app.Tests/media-vault-app.Tests.csproj `
        --filter "FullyQualifiedName~OpenTelemetryTests"
}
finally {
    Remove-Item Env:MEDIAVAULT_TEST_OTLP_EXPORT
    Remove-Item Env:OTEL_EXPORTER_OTLP_ENDPOINT
    Remove-Item Env:OTEL_EXPORTER_OTLP_PROTOCOL
}
```

The tests retain their in-memory assertions while also exporting the configured
signals to the dashboard. The deliberate unreachable-receiver test continues
to target its isolated loopback endpoint and proves collector failure does not
change the successful HTTP outcome.

## Inspect and correlate

Select service `MediaVault.TelemetryTests` for the deterministic test flows or
`MediaVault.API` for the running application.

1. On **Traces**, open `GET _test/telemetry/success`. Its inbound server span
   and outbound `GET` client span have the same trace ID. The outbound URL query
   value is redacted.
2. On **Structured logs**, find event ID `3999` (`TelemetryTestWarning`). Its
   trace and span identifiers point back to the success request.
3. The expected flow is a 404 request trace without unhandled event `3000`.
4. The outbound-failure flow has an error client span with HTTP status 503 and
   no secret query value.
5. The unexpected flow has a 500 request trace and exactly one event `3000`.
   The exported record has no exception object, message, stack trace, SQL, raw
   upstream body, password, token, or other deliberately planted secret text.
6. On **Metrics**, select `http.server.request.duration`,
   `http.client.request.duration`, and a `System.Runtime` metric. Dimensions use
   bounded HTTP/runtime attributes and contain no user or media identifier.

The dashboard batches incoming telemetry, so allow a few seconds for the final
records and metrics to appear.

## Stop and verify failure isolation

Stop the dashboard with `Ctrl+C` in its terminal. Leave the API running and
repeat:

```powershell
Invoke-WebRequest http://localhost:5210/openapi/v1.json
```

The response still returns 200. Exporter connection errors or dropped telemetry
must not change API status, response body, persistence, or business behavior.
Stop the API with `Ctrl+C` when finished.

## Troubleshooting

- **A port is already in use:** stop the conflicting process. If you customize
  dashboard endpoint flags, set the matching `OTEL_EXPORTER_OTLP_ENDPOINT` and
  protocol for the API rather than editing business code.
- **No telemetry appears:** confirm the dashboard is still running, the API uses
  `http-otel`, and gRPC uses port 4317. For the harness, confirm
  `MEDIAVAULT_TEST_OTLP_EXPORT=true` was set in the same terminal as
  `dotnet test`.
- **Only traces/metrics appear:** exported application logs intentionally include
  only `media_vault_app.*` Warning/Error records. Expected validation and
  not-found Results do not generate operational error logs.
- **The login URL expired or was lost:** stop and restart the dashboard to get a
  new transient token. Restarting clears the in-memory telemetry.
- **The dashboard is stopped:** exporter warnings are expected; the API remains
  available. Restart the dashboard to resume collection.
- **A proxy, firewall, or remote collector is involved:** this localhost guide
  no longer describes the security boundary. Configure transport and OTLP
  authentication deliberately and keep any `OTEL_EXPORTER_OTLP_HEADERS` value
  in environment or secret storage.

## References

- [Run the Aspire dashboard standalone](https://aspire.dev/dashboard/standalone/)
- [`aspire dashboard run` command](https://aspire.dev/reference/cli/commands/aspire-dashboard-run/)
- [Aspire dashboard security considerations](https://aspire.dev/dashboard/security-considerations/)
- [OpenTelemetry baseline](open-telemetry-baseline.md)
