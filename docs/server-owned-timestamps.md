# Server-owned timestamps

`CreatedAtUtc` and `UpdatedAtUtc` are persistence metadata owned by the API. Request DTOs do not accept either value. Application mappers therefore leave timestamp assignment to Infrastructure, and repositories apply the policy immediately before a write is persisted.

The lifecycle policy is:

- On create, both timestamps are set to the same UTC value from the registered `TimeProvider`.
- On a successful update with at least one non-timestamp field change, `CreatedAtUtc` is preserved and `UpdatedAtUtc` advances strictly beyond its previous value.
- No-op, failed, and cancelled operations do not persist a new `UpdatedAtUtc` value.
- Deletes do not change timestamps.
- TV-series graph writes apply the same rules to the media entry and each season. Adding a season initializes both values; updating a season preserves its creation value.

Current response contracts expose modification metadata where current media and profile responses can use it: `UpdatedAtUtc` is included in `UserDetailedDto`, `MediaEntryMinimalDto`, `MediaEntryDetailedDto`, and the existing season response DTOs. The fields are additive and remain server-to-client only. Optimistic-concurrency transport and conflict behavior are deferred to #168.

Tests inject a controllable `TimeProvider` into the Infrastructure policy so timestamp assertions are deterministic without changing production clock behavior.
