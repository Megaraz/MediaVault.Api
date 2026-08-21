# Optimistic concurrency contract

MediaVault uses a server-owned integer `Version` for optimistic concurrency on user profiles and media aggregates. The token is independent of `UpdatedAtUtc`: timestamps describe when data changed, while `Version` is the value clients must compare when submitting a mutation.

## Lifecycle and persistence

- New `User` and `MediaEntry` rows start at version `1`.
- Migration `AddOptimisticConcurrencyVersions` adds both non-null SQLite columns with a default of `1`, so existing rows are backfilled safely.
- A successful meaningful profile or media mutation increments the entity version exactly once. A TV-series mutation that changes its owned seasons increments the containing media aggregate version.
- No-op, failed, rejected, and cancelled operations do not advance the version.
- `Version` is configured as an EF Core concurrency token. Repositories first reject an already-stale submitted value and EF also compares the original version during `SaveChanges` to close the race with a concurrent writer.
- The API never retries, merges, or applies last-write-wins behavior after a conflict.

## HTTP contract

Profile and media response DTOs expose the current server-owned `version`. Media list/search responses include it so a client can retain the version associated with the item it displayed.

Protected mutations require the version the client previously read:

- `PUT /Auth` includes `expectedVersion` in `UserUpdateDto`.
- Every typed media `PUT` includes `expectedVersion` in its update DTO.
- `DELETE /MediaEntries/{id}` includes the required query parameter `expectedVersion`, for example `DELETE /MediaEntries/{id}?expectedVersion=3`.

Missing, zero, or negative body versions are validation failures. A missing delete query parameter is rejected by API model binding. A submitted version that no longer matches returns HTTP `409` with the normal safe `ErrorResponseBody`. Its stable code ends in `DatabaseConcurrencyFailure`; no database exception text or current stored values are disclosed.

Ownership checks remain authoritative. A caller cannot use a version token to update or delete another user's media entry, and a cross-user lookup retains the existing not-found behavior.

## Client rollout gate

This is a breaking mutation contract and must not be released until `Megaraz/MediaVault.Clients` coordinates both web and Android behavior. The exact client work is:

1. Add `version` to user/media response contracts and `expectedVersion` to user/media update contracts.
2. Extend the shared delete-media operation to send `expectedVersion` as a query parameter.
3. Preserve the latest server version through web edit/delete flows and Android service, model, and local-persistence mappings. Client-generated `UpdatedAtUtc` values must not be treated as concurrency tokens.
4. Handle `409` by telling the user the item changed and requiring a refresh/review; do not automatically retry a stale non-idempotent mutation.
5. Add shared-contract and app tests for successful version submission, stale conflicts, and refresh behavior.

The API and client pull requests should be merged before the next coordinated product release. There is no compatibility promise for older clients after the required-version API is released.

This concurrency foundation is a prerequisite for frequent simultaneous editing and future offline synchronization. It does not define offline authority, merge policy, tombstones, or conflict-resolution UI.
