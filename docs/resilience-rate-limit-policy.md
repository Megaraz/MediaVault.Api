# MediaVault request resilience and rate-limit policy

**Status:** Approved baseline, implemented and verified on 2026-08-14.

**Decision issue:** [#126](https://github.com/Megaraz/MediaVault.Api/issues/126)

**Roadmap parent:** [#64](https://github.com/Megaraz/MediaVault.Api/issues/64)

**Prerequisite:** #105 is complete. Runtime work begins only with #127, #128, and #129 in the order recorded in [section 13](#13-child-handoff-and-verification).

This is the durable policy for request cancellation, request budgets, outbound HTTP resilience, and targeted inbound rate limiting. It makes the boundary decisions visible before later children configure framework middleware or packages.

It deliberately separates three states:

- **Current** means checked-in behavior at #126 intake.
- **Approved target** means the contract later child issues must implement and test.
- **Deferred** means explicitly out of scope for v1.

Issue #126 made no runtime change. Issues #127 through #129 implemented this
approved baseline without changing existing routes, authentication,
authorization, persistence, or client success contracts. The integrated
verification record is [request-resilience-verification.md](request-resilience-verification.md).

## 1. Evidence and governing principles

The policy was reviewed against the checked-in .NET 10 application and the following sources on 2026-08-14:

- [ASP.NET Core rate limiting for .NET 10](https://learn.microsoft.com/aspnet/core/performance/rate-limit?view=aspnetcore-10.0) documents named endpoint policies, partitioning, zero queues, rejection handling, and the need to load-test configured limits.
- [ASP.NET Core request timeouts for .NET 10](https://learn.microsoft.com/aspnet/core/performance/timeouts?view=aspnetcore-10.0) documents that request timeouts are opt-in, can be endpoint-specific, cancel `HttpContext.RequestAborted`, and do not automatically abort a response.
- [.NET HTTP resilience guidance](https://learn.microsoft.com/dotnet/core/resilience/http-resilience) documents one resilience handler per client, the broad defaults included by the standard handler, and the danger of retrying unsafe HTTP methods. MediaVault will use the smallest custom pipeline instead of those broader defaults.
- [RAWG API documentation](https://rawg.io/apidocs) currently advertises up to 20,000 requests per month on its free plan. This is plan-specific and must be rechecked when the plan changes.
- [TMDB rate-limit documentation](https://developer.themoviedb.org/docs/rate-limiting) says its legacy fixed limit is disabled, that an upper limit around 40 requests per second can change, and that clients must respect HTTP 429.
- [Google Books API usage guidance](https://developers.google.com/books/docs/v1/using) identifies the API key as the project quota identity. [Google Cloud quota guidance](https://cloud.google.com/apis/docs/capping-api-usage) says actual limits are API/project-specific and must be inspected in the active Cloud project; no public numeric Google Books quota is treated as authoritative here.

The policy follows these rules:

1. Caller cancellation, MediaVault request-budget expiry, outbound attempt/total expiry, transport failure, upstream throttling, and local throttling are distinct outcomes.
2. A retry is allowed only when the concrete request is read-only and safe to repeat. Current provider calls are GETs; a future unsafe method is opt-out by default, not retryable by accident.
3. A local admission limit protects an API boundary; it is neither authorization, DDoS protection, nor a durable provider-quota ledger.
4. Existing one-event ownership, redaction, and Result-to-HTTP boundaries from `error-observability-policy.md` remain in force.
5. The approved v1 topology is a single API instance. Process-local state is intentional and must be visible in configuration and operations documentation.

## 2. Current boundary inventory

### 2.1 Current cross-cutting behavior

- Controllers accept ASP.NET Core's request cancellation token and pass it to Application services.
- Application services, repositories, EF Core terminal operations, and the three typed provider clients generally propagate that token. #127 must audit every path instead of relying on this inventory alone.
- `ApiClientBase.SendAndMapAsync` is the current provider classification boundary. It keeps caller cancellation distinct through the ResultPattern transport mapper, maps non-caller timeout/transport failures to the existing safe external-service Result, and emits the existing final provider event only when the result policy requires it.
- `ResultResponseMapper` maps ordinary Result failures to the established JSON `{ "message", "code" }` shape. Unexpected exceptions use the separate safe correlated `ProblemDetails` boundary.
- There is currently no explicit request-timeout middleware, inbound rate limiter, `HttpClient.Timeout` override, resilience handler, retry, or provider quota ledger. The platform/default behavior is not an approved MediaVault policy.

### 2.2 API route inventory

Every current controller action is included below. `CT` means its request token reaches the called service today; #127 owns the exhaustive code audit. `Budget` and `limit` are approved targets, not current middleware.

| Route | Auth | Work and side effect | Current cancellation path | Approved request budget / inbound policy |
|---|---|---|---|---|
| `POST /Auth/register` | Anonymous | Validates and creates a local account; non-idempotent | CT -> auth service -> repository/EF Core | `Authentication` 15 s; `RegistrationByIp` |
| `POST /Auth/login` | Anonymous | Validates credentials and creates a JWT response; no persistent write | CT -> auth service -> repository/EF Core | `Authentication` 15 s; `LoginByIp` |
| `PUT /Auth` | JWT | Updates current local user; non-idempotent | CT -> user service -> repository/EF Core | No v1 server budget or limiter |
| `GET /Auth/me` | JWT | Reads current local user | CT -> user service -> repository/EF Core | No v1 server budget or limiter |
| `GET /Users` | Currently anonymous; separate authorization concern | Lists users | CT -> user service -> repository/EF Core | No v1 server budget or limiter; do not use resilience work to hide the authorization gap |
| `GET /Users/{id}` | Currently anonymous; separate authorization concern | Reads a user | CT -> user service -> repository/EF Core | No v1 server budget or limiter; do not use resilience work to hide the authorization gap |
| `DELETE /Users/{id}` | Currently anonymous; separate authorization concern | Deletes a user; non-idempotent | CT -> user service -> repository/EF Core | No v1 server budget or limiter; do not use resilience work to hide the authorization gap |
| `POST /MediaEntries/movies` | JWT | Creates a movie entry; non-idempotent | CT -> write service -> repository/EF Core | No v1 server budget or limiter |
| `POST /MediaEntries/tv-series` | JWT | Creates a TV-series entry; non-idempotent | CT -> write service -> repository/EF Core | No v1 server budget or limiter |
| `POST /MediaEntries/games` | JWT | Creates a game entry; non-idempotent | CT -> write service -> repository/EF Core | No v1 server budget or limiter |
| `POST /MediaEntries/books` | JWT | Creates a book entry; non-idempotent | CT -> write service -> repository/EF Core | No v1 server budget or limiter |
| `POST /MediaEntries/manga` | JWT | Creates a manga entry; non-idempotent | CT -> write service -> repository/EF Core | No v1 server budget or limiter |
| `PUT /MediaEntries/movies/{id}` | JWT | Updates a movie entry; non-idempotent | CT -> write service -> repository/EF Core | No v1 server budget or limiter |
| `PUT /MediaEntries/tv-series/{id}` | JWT | Updates a TV-series entry; non-idempotent | CT -> write service -> repository/EF Core | No v1 server budget or limiter |
| `PUT /MediaEntries/games/{id}` | JWT | Updates a game entry; non-idempotent | CT -> write service -> repository/EF Core | No v1 server budget or limiter |
| `PUT /MediaEntries/books/{id}` | JWT | Updates a book entry; non-idempotent | CT -> write service -> repository/EF Core | No v1 server budget or limiter |
| `PUT /MediaEntries/manga/{id}` | JWT | Updates a manga entry; non-idempotent | CT -> write service -> repository/EF Core | No v1 server budget or limiter |
| `GET /MediaEntries/movies/{id}` | JWT | Reads owned movie entry | CT -> read service -> repository/EF Core | No v1 server budget or limiter |
| `GET /MediaEntries/tv-series/{id}` | JWT | Reads owned TV-series entry | CT -> read service -> repository/EF Core | No v1 server budget or limiter |
| `GET /MediaEntries/games/{id}` | JWT | Reads owned game entry | CT -> read service -> repository/EF Core | No v1 server budget or limiter |
| `GET /MediaEntries/books/{id}` | JWT | Reads owned book entry | CT -> read service -> repository/EF Core | No v1 server budget or limiter |
| `GET /MediaEntries/manga/{id}` | JWT | Reads owned manga entry | CT -> read service -> repository/EF Core | No v1 server budget or limiter |
| `POST /MediaEntries/search` | JWT | Searches owned local entries; read-only but POST-shaped | CT -> read service -> repository/EF Core | No v1 server budget or limiter; never infer retry eligibility from this route's POST method |
| `GET /MediaEntries/{id}` | JWT | Reads owned entry | CT -> read service -> repository/EF Core | No v1 server budget or limiter |
| `GET /MediaEntries` | JWT | Lists owned entries | CT -> read service -> repository/EF Core | No v1 server budget or limiter |
| `DELETE /MediaEntries/{id}` | JWT | Deletes owned entry; non-idempotent at the application boundary | CT -> write service -> repository/EF Core | No v1 server budget or limiter |
| `POST /RawgApi/search` | JWT | Searches RAWG metadata; outbound GET | CT -> API service -> RAWG typed client -> `HttpClient` | `ExternalMetadata` 20 s; `RawgMetadataByUser` |
| `GET /RawgApi/{id}` | JWT | Gets RAWG game metadata; outbound GET | CT -> API service -> RAWG typed client -> `HttpClient` | `ExternalMetadata` 20 s; `RawgMetadataByUser` |
| `POST /TmdbApi/movie/search` | JWT | Searches TMDB movies; outbound GET | CT -> API service -> TMDB typed client -> `HttpClient` | `ExternalMetadata` 20 s; `TmdbMetadataByUser` |
| `GET /TmdbApi/movie/{id}` | JWT | Gets TMDB movie metadata; outbound GET | CT -> API service -> TMDB typed client -> `HttpClient` | `ExternalMetadata` 20 s; `TmdbMetadataByUser` |
| `POST /TmdbApi/tv/search` | JWT | Searches TMDB TV series; outbound GET | CT -> API service -> TMDB typed client -> `HttpClient` | `ExternalMetadata` 20 s; `TmdbMetadataByUser` |
| `GET /TmdbApi/tv/{id}` | JWT | Gets TMDB TV-series metadata; outbound GET | CT -> API service -> TMDB typed client -> `HttpClient` | `ExternalMetadata` 20 s; `TmdbMetadataByUser` |
| `POST /GoogleBooksApi/search` | JWT | Searches Google Books metadata; outbound GET | CT -> API service -> Google Books typed client -> `HttpClient` | `ExternalMetadata` 20 s; `GoogleBooksMetadataByUser` |
| `GET /GoogleBooksApi/{volumeId}` | JWT | Gets Google Books volume metadata; outbound GET | CT -> API service -> Google Books typed client -> `HttpClient` | `ExternalMetadata` 20 s; `GoogleBooksMetadataByUser` |

## 3. Outcome taxonomy and public contracts

The child implementations must classify by cancellation-token ownership and response state, never merely by the exception type. `TaskCanceledException` alone cannot prove which budget cancelled it.

| Condition | Client response | Result/log/metric ownership |
|---|---|---|
| Caller disconnects or cancels `RequestAborted` before a MediaVault budget expires | Usually no observable response; do not manufacture 499 | Propagate cancellation; no Result conversion and no owned Warning/Error event |
| `Authentication` or `ExternalMetadata` server request budget expires | `504 application/json` with `{ "message": "The request timed out. Please try again.", "code": "Request.Timeout" }`, unless the response is already started or the caller disconnected | The API timeout response writer owns one correlated Warning event `ServerRequestTimeout` (event ID 3001) and a bounded timeout metric. The exception handler and provider client do not duplicate it |
| Outbound attempt timeout or total timeout, while the caller/server request is still active | Existing safe external-service Result and its existing Result-to-HTTP mapping (currently 503 for cancelled transport failures) | The provider classification boundary ultimately owns event 2100 once; attempt telemetry is metric-only |
| DNS/connect/reset or other `HttpRequestException` | Existing safe external-service Result/contract | `ApiClientBase` owns final event 2100 once |
| Upstream provider returns HTTP 429 | Existing provider-safe Result/contract; it remains distinguishable from a local response by its existing external-service message/code and non-429 MediaVault mapping | `ApiClientBase` owns final event 2100 once and records provider/status safely; no raw provider body |
| MediaVault named limiter rejects a request | `429 application/json` with `{ "message": "Too many requests. Please try again later.", "code": "Request.RateLimited" }` | Rate-limit rejection callback owns one correlated Warning event `InboundRateLimitRejected` (event ID 3002) and a bounded rejection metric |
| Validation, authentication/authorization, ordinary 4xx, known database failure, or unexpected exception | Existing contracts unchanged | Existing policy ownership remains unchanged |

`Retry-After` is included only for a local 429 when the limiter lease provides an estimate. It is an integer delta-seconds header, rounded up to at least `1`; no fabricated value is emitted when no estimate is available. The response content type is `application/json; charset=utf-8` through the normal API JSON formatter.

The timeout and local-429 values above are implemented contracts with OpenAPI
metadata and deterministic contract tests. The historical `RateLimit.Exceeded`
placeholder is corrected here to the delivered `Request.RateLimited` code.

## 4. Server request budgets and cancellation ownership

### 4.1 Approved named budgets

| Policy | Endpoints | Duration | Why |
|---|---|---:|---|
| `Authentication` | `POST /Auth/register`, `POST /Auth/login` | 15 seconds | Bounds password/hash/database work while allowing normal local development and a deliberate credential check |
| `ExternalMetadata` | All eight RAWG, TMDB, and Google Books endpoints in section 2 | 20 seconds | Contains outbound total budget, mapping, serialization, and small local overhead |
| No v1 timeout policy | All other endpoints | Not applied | Do not impose an unmeasured global timeout on CRUD or the separate Users authorization issue |

The framework request-timeout middleware must be endpoint-specific. It must use its timeout token plus `RequestAborted` ownership/state to detect a server budget. When it expires, it cancels downstream work, writes the approved 504 only if a response can still be written, and avoids a second exception-boundary response/event.

### 4.2 Implemented request-timeout configuration

Issue #127 configures the two named policies through the `RequestTimeouts` section. The production defaults are `AuthenticationMilliseconds: 15000` and `ExternalMetadataMilliseconds: 20000`; values must be positive and no greater than ten minutes, and invalid startup configuration fails validation. Tests may override these values through test configuration and hold downstream work behind a cancellation-aware task, rather than using provider outages or long wall-clock waits.

The timeout response writer is the sole owner of event `3001` and the `mediavault.request_timeouts` counter. The framework invokes it only for an expired request-timeout policy; it writes the approved JSON 504 only while the response remains writable. The exception boundary continues to preserve caller cancellation and therefore does not own a duplicate timeout event.

Middleware ordering for the later implementation is: exception handler, CORS, routing, authentication, named rate limiter, named request timeout middleware, authorization, then controller endpoints. This gives endpoint-aware middleware routing metadata, gives authenticated provider policies a validated principal, and prevents authorization from executing after a rejected request. #127/#129 must verify the final framework-compatible order with integration tests.

## 5. Outbound provider timeout and retry policy

### 5.1 Current operations and quota evidence

| Provider | Current operations | Method / idempotency | Quota or term evidence | Approved v1 consequence |
|---|---|---|---|---|
| RAWG | game search, game by ID | GET; read-only and retry-eligible | Free-plan documentation currently says 20,000 requests/month; plan and terms can change | Apply a deliberately low per-user inbound limit and never present it as the provider's monthly accounting |
| TMDB | movie search/detail, TV search/detail | GET; read-only and retry-eligible | Official guidance says a changeable upper limit around 40 requests/second and to honor 429 | Stay far below the advisory upper limit per user; honor bounded `Retry-After` |
| Google Books | volume search/detail | GET; read-only and retry-eligible | The API key identifies the quota project; active Console quota is authoritative | Use a conservative local limit until the active project quota is recorded; no numeric public quota is assumed |

All current outbound operations issue a GET even where MediaVault's inbound search route is POST. Retry eligibility is decided at the outbound operation, not inferred from the inbound route.

### 5.2 Provider pipeline defaults

Each typed client receives exactly one custom `Microsoft.Extensions.Http.Resilience` pipeline in #128. It contains only a total timeout, a retry strategy, and an attempt timeout; it does not use the standard handler because that also adds outbound concurrency limiting and circuit breaking. It does not stack a second resilience handler.

| Setting | RAWG | TMDB | Google Books |
|---|---:|---:|---:|
| Attempt timeout | 5 s | 5 s | 5 s |
| Total outbound timeout | 12 s | 12 s | 12 s |
| Maximum retry attempts after the first send | 1 | 1 | 1 |
| Backoff | Exponential, base 500 ms, jitter | Exponential, base 500 ms, jitter | Exponential, base 500 ms, jitter |
| Maximum ordinary retry delay | 1 s | 1 s | 1 s |
| Maximum accepted `Retry-After` | 2 s | 2 s | 2 s |

At defaults, two 5-second attempts plus the largest permitted retry wait is at most 12 seconds. The pipeline's total timeout is the hard bound; the 20-second inbound policy retains eight seconds for routing, authentication, Result mapping, response serialization, and an orderly timeout response. Jitter must never raise a selected delay above the configured maximum. A valid provider `Retry-After` is used only when it is no greater than two seconds and fits in the remaining total budget; otherwise the request finishes as a final safe failure rather than sleeping past the budget.

The later implementation must set the typed clients' broad `HttpClient.Timeout` to infinite (or otherwise prove it cannot win) so one explicit policy owns outbound timeout classification. It must not rely on the platform's broad default timeout.

### 5.3 Implemented provider resilience configuration

Issue #128 implements this policy with stable
`Microsoft.Extensions.Http.Resilience` 10.9.0, verified against the current
NuGet package and .NET HTTP resilience guidance on 2026-08-14. Its current
dependency floor requires the stable `Microsoft.Extensions.Configuration`
10.0.11 patch in Infrastructure. Each typed client has exactly one custom
pipeline ordered as total timeout, retry, then attempt
timeout; the broader standard handler is not registered. `HttpClient.Timeout`
is infinite so the explicit pipeline owns outbound timeout behavior.

Configuration binds once at startup from
`RequestResilience:Providers:{Rawg|Tmdb|GoogleBooks}` using millisecond values
for deterministic development/test overrides. Validation requires the single
approved retry, positive values, base delay no greater than maximum delay, the
two attempts plus the larger delay cap to fit within the provider total, and
the provider total to remain below the enclosing external-metadata request
budget. Retry callbacks emit only the low-cardinality
`mediavault.external_provider.retries` counter and retry-delay histogram; final
failure event 2100-2102 ownership remains in `ApiClientBase`. The built-in
`Polly` logging category is disabled because its per-attempt Warning/Error
events would duplicate that ownership; retry visibility remains in the bounded
metrics and normal outbound trace.

### 5.4 Retry matrix

| Outcome | Retry? | Reason |
|---|---|---|
| Current provider GET receives 408, 429, 500, 502, 503, or 504 | Yes, once, within the total budget | Explicitly transient/read-only candidates |
| `HttpRequestException` | Yes, once, within the total budget | Connection/transport failure can be transient |
| Resilience attempt timeout | Yes, once, within the total budget | A single slow attempt may recover |
| Caller cancellation | No | The caller withdrew the work |
| Server request budget expiry | No | The enclosing request no longer has budget |
| 400, 401, 403, 404, 409, 422, validation/authentication/authorization failure | No | Retrying cannot make a bad/authenticated/authorized request valid and could amplify a security fault |
| Malformed/oversized/invalid successful payload | No | Repeating untrusted invalid content is not a transient recovery policy |
| Any current or future POST, PUT, PATCH, DELETE, CONNECT, or other unsafe outbound method | No by default | No duplicate-write risk is accepted without a separately documented idempotency design |
| Future explicitly idempotent write | Not approved by this policy | Requires its own operation-level idempotency key/semantics, provider documentation, and decision record |

No v1 circuit breaker, hedging, fallback, cache, database retry, or durable monthly quota ledger is approved.

## 6. Targeted inbound rate limits

Only named endpoint policies are approved. There is no global limiter and ordinary authenticated MediaVault CRUD remains untouched.

| Policy | Endpoints | Partition | Algorithm and initial limit | Queue |
|---|---|---|---|---:|
| `LoginByIp` | `POST /Auth/login` | Remote IP address | Fixed window: 10 permits per 10 minutes | 0 |
| `RegistrationByIp` | `POST /Auth/register` | Remote IP address | Fixed window: 3 permits per 60 minutes | 0 |
| `RawgMetadataByUser` | RAWG search/detail | Stable authenticated MediaVault user ID plus provider name | Fixed window: 20 permits per 60 minutes | 0 |
| `TmdbMetadataByUser` | TMDB search/detail | Stable authenticated MediaVault user ID plus provider name | Token bucket: capacity 20, replenish 20 each 60 seconds | 0 |
| `GoogleBooksMetadataByUser` | Google Books search/detail | Stable authenticated MediaVault user ID plus provider name | Fixed window: 10 permits per 60 minutes | 0 |

Remote IP means the direct connection peer in v1. The API must not trust `X-Forwarded-For` or any other forwarding header until a concrete hosting topology identifies and configures trusted proxies/networks. A missing direct peer address uses one explicit low-trust fallback partition, never an attacker-supplied header.

The user partition uses the validated JWT claim already used for owner isolation. It is never taken from a route, body, query string, or client-provided header. The partition key itself is not emitted in logs, traces, or metric dimensions.

These limits are conservative initial admission controls for a personal single-instance application, not published provider guarantees. The RAWG setting deliberately stays below an always-on single-user pace that would exhaust its stated 20,000/month free-plan allowance, while still allowing normal search/detail use. TMDB's per-user rate is far below its changeable advisory upper limit. Google Books remains conservative until the active project's Console quota is reviewed. Load evidence may revise numeric values without changing the partitioning or public local-429 contract.

## 7. Configuration, validation, and reload

The implemented configuration uses the following shape and invariants:

- `RequestTimeouts` contains `AuthenticationMilliseconds` and `ExternalMetadataMilliseconds`.
- `RequestResilience:Providers:{Rawg|Tmdb|GoogleBooks}` contains attempt timeout, total timeout, retry count, base/max delay, and maximum accepted `Retry-After`.
- `RateLimiting:{LoginByIp|RegistrationByIp|RawgMetadataByUser|TmdbMetadataByUser|GoogleBooksMetadataByUser}` contains the numeric limiter parameters and must retain queue length zero.

Options bind through typed options and validate on startup. Invalid/missing production configuration fails fast; it must not silently fall back to unbounded retries or default framework limits. Validation requires positive durations/counts, queue length exactly zero, maximum retry attempts no greater than one, attempt timeout no greater than total timeout, and a provider worst case (including the larger of ordinary delay and accepted `Retry-After`) no greater than its total timeout and enclosing 20-second request budget.

Production behavior does not hot-reload resilience or limiter configuration in v1. A process restart applies a reviewed configuration change, which keeps a running policy coherent and testable. Development/test settings may use shorter, isolated values only through environment/test configuration; tests must not require a live provider or consume a real quota.

## 8. Observability, privacy, and one-event ownership

OpenTelemetry and the existing structured logging policy remain the signal foundation. Add only low-cardinality fields:

| Event/metric | Owner | Safe dimensions | Prohibited data |
|---|---|---|---|
| `ServerRequestTimeout` / request-timeout counter (3001) | API timeout response writer | named policy, normalized route, outcome | user ID, IP, token, request body, query text |
| `InboundRateLimitRejected` / rejection counter (3002) | rate-limiter rejection callback | named policy, normalized route, limiter algorithm, retry-after-present | partition key, user ID, IP, token, request body |
| retry counter/histogram | resilience pipeline callback | provider, attempt number, failure kind, final-or-retry | URL/query/API key, response body, user ID |
| final provider failure (existing 2100/2101/2102) | `ApiClientBase` classification boundary | existing provider/status/failure fields | raw provider body, API key, URL query, user ID |

The retry callback must not emit Warning/Error for every attempt. It emits bounded telemetry; after exhaustion the existing final provider classification emits the single owned event. A local rate-limit rejection must never be logged by a controller, Application service, provider client, and middleware together. Caller cancellation remains unlogged. Exception details and telemetry redaction follow `error-observability-policy.md`.

## 9. Compatibility and security posture

- The existing provider-safe Result messages and mapping remain the owner of upstream errors. An upstream 429 is not rewritten to MediaVault's local 429 contract.
- Local 429 and server-timeout bodies are implemented MediaVault contracts and are included in generated OpenAPI coverage. The local 429 remains distinct from an upstream provider 429.
- Rate limiting does not authenticate, authorize, prevent all DDoS, correct the `UsersController` authorization concern, or persist a monthly quota. A rejected request is not evidence a caller would otherwise be authorized.
- No secret, JWT, provider key, raw query, raw upstream body, email, or stable user identifier may enter the new policy's logs/traces/metric labels. RAWG/Google API keys remain backend-only even though current request construction uses query parameters.
- Process-local limiter state resets on restart and is independent in each API instance. A malicious source can create partitions by using many IPs; this is why the v1 policy is a targeted admission control rather than a DDoS claim.

## 10. Explicit deferred work and revision triggers

The following are not v1 behavior: distributed limiter state, Redis, reverse-proxy forwarding configuration, global limits, durable provider quota accounting/alerting, circuit breaking, hedging, fallback, provider-response caching, database retries, WAF/DDoS controls, or client-side retry changes.

The policy must be revised before any of these conditions are introduced:

1. More than one API instance, autoscaling, or restarts make per-process limits materially misleading: select a distributed store/limiter and document availability/failure semantics.
2. A reverse proxy/load balancer/CDN is introduced: identify trusted proxy addresses/networks, configure forwarded headers, and test spoofed-header rejection before using client IP partitions.
3. A provider plan, quota, endpoint set, or pricing changes: refresh dated provider evidence and revise the provider limit/budget deliberately.
4. Observed local rejections, timeout rate, provider 429s, or retry exhaustion materially affect normal use: review measured low-cardinality telemetry before increasing limits or attempts.
5. An outbound unsafe method, batch/costly endpoint, or user-visible idempotent write is added: create a new operation policy rather than inheriting GET retry rules.
6. Production hosting/monitoring is selected: define durable quota reporting, alerting, retention, and cost limits separately from this local process policy.

## 11. Implementation sequence

1. #127 completed token propagation, added only the two named request budgets, proved caller/server distinction, and implemented/tested the approved 504 contract.
2. #128 added the minimum provider-specific outbound resilience handler and typed validated options, preserving `ApiClientBase` final classification/contract ownership.
3. #129 implemented the five targeted named ASP.NET Core policies, response callback, local 429 OpenAPI/test coverage, and ordering verification. It proceeded independently after #126 while preserving this document's approved boundaries.
4. #130 completed the integrated policy/contract/telemetry/configuration audit and documented only verified behavior.

## 12. Deterministic test coverage

- Already-cancelled and mid-operation caller cancellation across controller, service, EF Core, and each provider client; no result conversion or operational event.
- Server request timeout while work is deliberately held; downstream cancellation, safe 504 when writable, no response after disconnect/response start, and exactly one 3001 event.
- Per-provider first-attempt success, transient retry-then-success, retry exhaustion, non-retryable 4xx, capped/over-budget `Retry-After`, attempt timeout, total timeout, and one final 2100-class event.
- Login/registration remote-IP partition isolation, each provider/user partition isolation, zero queue behavior, replenishment, direct-peer/fallback partition behavior, and no trusted forwarded-header behavior before configuration.
- Exact local 429 body/content type/code/header behavior; upstream 429 remains distinct; OpenAPI includes target timeout/429 responses.
- Startup option-validation failures and isolated development/test overrides without wall-clock flakiness or real provider traffic.
- Metrics/logging dimensions contain policy/provider/route only and omit partition/user/IP/key/query/body values.

## 13. Child handoff and verification

The final child order is unchanged:

1. #126 — Define MediaVault resilience and rate-limit boundary policies
2. #127 — Complete end-to-end cancellation and enforce request budgets (after #126)
3. #128 — Add provider-specific outbound HTTP timeout and retry policies (after #127)
4. #129 — Add targeted inbound rate limiting and a stable 429 contract (after #126; may proceed alongside #127/#128)
5. #130 — Verify and document the integrated request-resilience foundation (after #127-#129)

## Integrated verification

```powershell
rg -n "CancellationToken|AddHttpClient|HttpClient|SendAsync|SaveChangesAsync|ToListAsync|FirstOrDefaultAsync|Authorize|AllowAnonymous" -g '*.cs'
dotnet test media-vault-app.slnx
git diff --check
```

The integrated review confirms every section-2 route, all three providers, each
taxonomy outcome, the default timing proof, exact limiter partitions/numbers,
provider evidence, one-event ownership, and every scaling trigger. See the
dated [integrated verification record](request-resilience-verification.md) for
the executable evidence and remaining deployment checks.
