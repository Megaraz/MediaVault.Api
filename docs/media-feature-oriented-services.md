# Feature-oriented media services

Media entry operations use explicit Application contracts rather than the generic dependent-entity CRUD path.

## Application boundary

- `IMediaEntryReadService` owns authenticated-owner validation, owner existence checks, pagination normalization, search validation, and typed detail reads.
- `IMediaEntryWriteService` exposes named create and update operations for movies, TV series, games, books, and manga, plus version-checked deletion.
- `MediaEntryMapping` contains the subtype-specific DTO/entity mappings. It is an application-owned static mapping policy and is not registered as a generic mapper service.

## Persistence boundary

`IMediaEntryRepo` contains narrow media operations: detailed loading, minimal list/search projections, creation, one update method per media subtype, and owned version-checked deletion. `MediaEntryRepo` owns the EF Core details for those operations.

Subtype persistence behavior remains explicit:

- detailed reads include TV seasons;
- TV updates merge owned seasons and advance the media aggregate version when season data changes;
- game updates replace the `PcRequirements` value object;
- all writes preserve owner isolation, server-owned timestamps, cancellation, and optimistic-concurrency failures.

The API routes, JSON DTOs, Result/ProblemDetails mapping, and authenticated ownership behavior remain unchanged. The generic dependent-entity service/repository bases and media mapper contracts have no active media callers; the remaining broader SharedKernel generic cleanup belongs to issue #171.
