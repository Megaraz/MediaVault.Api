# Request resilience and rate-limit verification

**Issue:** [#130](https://github.com/Megaraz/MediaVault.Api/issues/130)
**Verified:** 2026-08-14
**Scope:** request cancellation and budgets, provider resilience, targeted
inbound limits, API contracts, configuration, telemetry, and client
compatibility

## Outcome

MediaVault now has a deliberately narrow single-instance request-resilience
baseline. It propagates caller cancellation, applies named server budgets only
to login/registration and external metadata endpoints, bounds each provider GET
with one explicit total-timeout/retry/attempt-timeout pipeline, and applies
named zero-queue admission limits only where cost or abuse risk justifies them.

The audit found and corrected one documentation-only drift: the approved-policy
placeholder for the local 429 code was `RateLimit.Exceeded`, while the merged
middleware and its contract test consistently deliver `Request.RateLimited`.
The checked-in contract is now documented; no runtime or client contract changed
in this issue.

## Verified outcomes and ownership

| Condition | Observable API outcome | Single final diagnostic owner |
| --- | --- | --- |
| Caller cancellation/disconnect | Cancellation is propagated; no synthetic 499/500/504 response | None |
| `Authentication` or `ExternalMetadata` budget expires | `504 application/json` with `{ "message": "The request timed out. Please try again.", "code": "Request.Timeout" }` | API timeout callback, event 3001 |
| Provider attempt/total timeout or transport failure | Existing safe external-service Result mapping (503 for cancelled transport) | `ApiClientBase`, event 2100 |
| Provider 429 or exhausted transient 5xx | Existing provider-safe Result mapping, not the local 429 body | `ApiClientBase`, event 2100-2102 as classified |
| Local named limiter rejection | `429 application/json` with `{ "message": "Too many requests. Please try again later.", "code": "Request.RateLimited" }`; `Retry-After` only when the lease supplies it | Rate-limiter callback, event 3002 |

The timeout and limiter callbacks emit only policy/boolean metadata. Provider
retry telemetry contains provider, attempt, and failure kind; it excludes
partition keys, raw user IDs/IPs, tokens, URLs/query values, request/provider
bodies, and credentials. Polly's per-attempt Warning/Error logs are disabled so
the retry pipeline cannot duplicate final failure ownership.

## Boundaries, budgets, and limits

- `POST /Auth/register` and `POST /Auth/login` use the 15-second
  `Authentication` request budget. Registration and login partition by the
  direct connection IP, with fixed zero-queue limits of 3 per hour and 10 per
  10 minutes respectively.
- The eight RAWG, TMDB, and Google Books metadata endpoints use the 20-second
  `ExternalMetadata` request budget. They partition by the validated JWT user
  ID: RAWG is fixed-window 20/hour, TMDB is token-bucket capacity/replenishment
  20/minute, and Google Books is fixed-window 10/hour. Ordinary CRUD is
  unmarked and unaffected.
- Each typed provider client has exactly one custom pipeline: 12-second total
  timeout, at most one retry after the initial GET, 5-second attempt timeout,
  jittered 500 ms to 1-second ordinary backoff, and a 2-second accepted
  `Retry-After` cap. Two 5-second attempts plus the largest allowed 2-second
  delay fit within 12 seconds, leaving eight seconds in the enclosing request
  budget. Options validation rejects configurations that violate this proof.
- Only GET operations retry, and only for 408, 429, 500, 502, 503, 504,
  `HttpRequestException`, or an attempt timeout. Caller/server cancellation,
  authentication/authorization/validation errors, ordinary non-transient 4xx,
  malformed payloads, and all unsafe methods do not retry.

The API uses `RemoteIpAddress` directly. It does not enable or trust forwarded
client-IP headers; a missing direct peer gets one explicit fallback partition.
Limiter state is process-local, resets on restart, and is independent per API
instance.

## Configuration and reproducible checks

`appsettings.example.json` documents `RequestTimeouts`,
`RequestResilience:Providers`, and `RateLimiting`. The options bind and validate
on startup. Changes require a restart; test hosts use isolated configuration
overrides and controlled handlers/tokens, not live provider traffic.

Run the clean verification from the repository root:

```powershell
dotnet clean media-vault-app.slnx
dotnet restore media-vault-app.slnx
dotnet build media-vault-app.slnx --no-restore
dotnet test media-vault-app.slnx --no-build --no-restore
dotnet list media-vault-app.slnx package --include-transitive
git diff --check
```

Focused executable evidence includes `ExceptionBoundaryTests` for caller versus
server-budget cancellation and event 3001 ownership; `ProviderHttpResilienceTests`
for transient and unsafe-method retry decisions, capped `Retry-After`, attempt
timeouts, total-budget validation, telemetry, redaction, and final event
ownership; `RateLimitingTests` for local 429 body/header, direct-peer behavior,
user partition isolation, and unaffected endpoints; controller metadata tests
for every budgeted/limited route; and generated OpenAPI contract tests for
timeout/429 schemas.

The full clean verification passed on the date above with no new warning. The
pre-existing warnings are NuGet advisories for `Microsoft.OpenApi` 2.0.0 and
`SQLitePCLRaw.lib.e_sqlite3` 2.1.11 (high), `NuGet.Packaging` and
`NuGet.Protocol` 6.12.1 (low), plus the existing test-project EF Core
10.0.9/10.0.10 assembly-version conflict. Package remediation is deliberately
separate; this issue adds no package.

## Provider evidence and client compatibility

- [RAWG documentation](https://rawg.io/apidocs) and
  [terms](https://rawg.io/tos_api) were rechecked on 2026-08-14: its free plan
  advertises up to 20,000 requests/month and attribution requirements. The
  local per-user limit is admission control, not monthly quota accounting.
- [TMDB's rate-limit guidance](https://developer.themoviedb.org/docs/rate-limiting)
  still describes a changeable upper limit around 40 requests/second and says
  consumers must respect 429 responses. The local limit remains deliberately
  lower and the retry pipeline caps provider `Retry-After`.
- [Google Books documentation](https://developers.google.com/books/docs/v1/using)
  confirms public requests identify the project with an API key. The active
  Google Cloud project's quota remains console-only and was not queried by this
  repository audit; review it before deployment or a plan/quota change, then
  record the project-specific value outside source control.

The current MediaVault.Clients shared HTTP mapping accepts the unchanged
`{ message, code }` error-body shape and maps HTTP 429 as a normal failure, so
web and Android require no change. Neither client currently presents a retry
countdown from `Retry-After`; that is a separately scoped UX decision, not a
backend compatibility gap.

## Deployment triggers and non-goals

Revisit this policy before introducing a reverse proxy/CDN (configure and test
trusted forwarded headers), multiple API instances/autoscaling (select
distributed limiter state), a provider plan/quota/endpoint change, material
rejection or retry/timeout telemetry, outbound unsafe operations, circuit
breaking/hedging/fallback, or production quota monitoring and alerting.

This baseline is not DDoS protection, a WAF, authorization remediation, a
durable quota ledger, distributed infrastructure, production hosting, client
server-state retry, offline sync, or AI resilience. The separate
`UsersController` authorization concern remains out of scope.
